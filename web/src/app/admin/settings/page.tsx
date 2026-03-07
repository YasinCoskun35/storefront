"use client";

import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Switch } from "@/components/ui/switch";
import { toast } from "sonner";
import { Loader2, Save, Settings2 } from "lucide-react";
import { settingsApi, AppSetting } from "@/lib/api/settings";

interface SettingEditorProps {
  setting: AppSetting;
  value: string;
  onChange: (value: string) => void;
}

function SettingEditor({ setting, value, onChange }: SettingEditorProps) {
  switch (setting.dataType) {
    case "boolean":
      return (
        <div className="flex items-center justify-between">
          <div className="space-y-0.5">
            <Label>{setting.displayName}</Label>
            {setting.description && (
              <p className="text-sm text-muted-foreground">{setting.description}</p>
            )}
          </div>
          <Switch
            checked={value.toLowerCase() === "true"}
            onCheckedChange={(checked: boolean) => onChange(checked.toString())}
          />
        </div>
      );
    case "number":
      return (
        <div className="space-y-2">
          <Label htmlFor={setting.key}>{setting.displayName}</Label>
          {setting.description && (
            <p className="text-sm text-muted-foreground">{setting.description}</p>
          )}
          <Input
            id={setting.key}
            type="number"
            value={value}
            onChange={(e) => onChange(e.target.value)}
          />
        </div>
      );
    default:
      return (
        <div className="space-y-2">
          <Label htmlFor={setting.key}>{setting.displayName}</Label>
          {setting.description && (
            <p className="text-sm text-muted-foreground">{setting.description}</p>
          )}
          <Input
            id={setting.key}
            value={value}
            onChange={(e) => onChange(e.target.value)}
          />
        </div>
      );
  }
}

export default function AdminSettingsPage() {
  const queryClient = useQueryClient();
  const [modifiedSettings, setModifiedSettings] = useState<Record<string, string>>({});

  const { data: settings, isLoading } = useQuery({
    queryKey: ["settings"],
    queryFn: settingsApi.getAll,
  });

  const updateMutation = useMutation({
    mutationFn: async () => {
      const updates = Object.entries(modifiedSettings).map(([key, value]) =>
        settingsApi.update(key, value)
      );
      await Promise.all(updates);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["settings"] });
      setModifiedSettings({});
      toast.success("Settings saved successfully");
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || "Failed to save settings");
    },
  });

  const handleSettingChange = (key: string, value: string) => {
    setModifiedSettings((prev) => ({ ...prev, [key]: value }));
  };

  const handleSave = () => {
    if (Object.keys(modifiedSettings).length === 0) {
      toast.info("No changes to save");
      return;
    }
    updateMutation.mutate();
  };

  const hasChanges = Object.keys(modifiedSettings).length > 0;

  // Group settings by category
  const groupedSettings = settings?.reduce((acc, setting) => {
    if (!acc[setting.category]) {
      acc[setting.category] = [];
    }
    acc[setting.category].push(setting);
    return acc;
  }, {} as Record<string, AppSetting[]>);

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <Loader2 className="h-8 w-8 animate-spin text-primary" />
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="font-display text-3xl font-bold text-secondary flex items-center gap-2">
            <Settings2 className="h-8 w-8" />
            Application Settings
          </h1>
          <p className="text-muted-foreground">Manage features and configuration</p>
        </div>
        {hasChanges && (
          <Button onClick={handleSave} disabled={updateMutation.isPending} size="lg">
            {updateMutation.isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
            <Save className="mr-2 h-4 w-4" />
            Save Changes ({Object.keys(modifiedSettings).length})
          </Button>
        )}
      </div>

      <div className="grid gap-6">
        {groupedSettings &&
          Object.entries(groupedSettings).map(([category, categorySettings]) => (
            <Card key={category}>
              <CardHeader>
                <CardTitle>{category}</CardTitle>
                <CardDescription>
                  {category === "Features"
                    ? "Toggle application features on/off"
                    : category === "General"
                    ? "General application configuration"
                    : `${category} settings`}
                </CardDescription>
              </CardHeader>
              <CardContent className="space-y-6">
                {categorySettings.map((setting) => (
                  <SettingEditor
                    key={setting.key}
                    setting={setting}
                    value={modifiedSettings[setting.key] ?? setting.value}
                    onChange={(value) => handleSettingChange(setting.key, value)}
                  />
                ))}
              </CardContent>
            </Card>
          ))}
      </div>

      {hasChanges && (
        <div className="fixed bottom-6 right-6 z-50">
          <Button onClick={handleSave} disabled={updateMutation.isPending} size="lg" className="shadow-lg">
            {updateMutation.isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
            <Save className="mr-2 h-4 w-4" />
            Save {Object.keys(modifiedSettings).length} Change{Object.keys(modifiedSettings).length !== 1 ? "s" : ""}
          </Button>
        </div>
      )}
    </div>
  );
}

