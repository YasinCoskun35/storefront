"use client";

import { OrderStatus } from "@/lib/api/orders";
import { Check } from "lucide-react";

interface OrderTimelineProps {
  currentStatus: string | number;
  createdAt: string;
  submittedAt?: string;
  confirmedAt?: string;
}

export function OrderTimeline({
  currentStatus,
  createdAt,
  submittedAt,
  confirmedAt,
}: OrderTimelineProps) {
  // Convert string to enum if needed
  const statusValue = typeof currentStatus === "string" 
    ? OrderStatus[currentStatus as keyof typeof OrderStatus] 
    : currentStatus;

  const steps = [
    { status: OrderStatus.Pending, label: "Order Placed", date: submittedAt || createdAt },
    { status: OrderStatus.QuoteSent, label: "Quote Sent", date: null },
    { status: OrderStatus.Confirmed, label: "Confirmed", date: confirmedAt },
    { status: OrderStatus.Preparing, label: "Preparing", date: null },
    { status: OrderStatus.QualityCheck, label: "Quality Check", date: null },
    { status: OrderStatus.ReadyToShip, label: "Ready to Ship", date: null },
    { status: OrderStatus.Shipping, label: "Shipping", date: null },
    { status: OrderStatus.Delivered, label: "Delivered", date: null },
  ];

  const isStepComplete = (stepStatus: OrderStatus) => {
    return statusValue >= stepStatus;
  };

  const isStepCurrent = (stepStatus: OrderStatus) => {
    return statusValue === stepStatus;
  };

  return (
    <div className="py-6">
      <div className="flex items-center justify-between">
        {steps.map((step, index) => (
          <div key={step.status} className="flex flex-col items-center flex-1">
            <div className="flex items-center w-full">
              {index > 0 && (
                <div
                  className={`flex-1 h-0.5 ${
                    isStepComplete(step.status) ? "bg-blue-600" : "bg-gray-200"
                  }`}
                />
              )}
              <div
                className={`flex items-center justify-center w-10 h-10 rounded-full border-2 ${
                  isStepComplete(step.status)
                    ? "bg-blue-600 border-blue-600"
                    : isStepCurrent(step.status)
                    ? "bg-white border-blue-600"
                    : "bg-white border-gray-200"
                }`}
              >
                {isStepComplete(step.status) && !isStepCurrent(step.status) ? (
                  <Check className="w-5 h-5 text-white" />
                ) : (
                  <span
                    className={`text-sm font-medium ${
                      isStepCurrent(step.status) ? "text-blue-600" : "text-gray-400"
                    }`}
                  >
                    {index + 1}
                  </span>
                )}
              </div>
              {index < steps.length - 1 && (
                <div
                  className={`flex-1 h-0.5 ${
                    isStepComplete(steps[index + 1].status) ? "bg-blue-600" : "bg-gray-200"
                  }`}
                />
              )}
            </div>
            <div className="mt-2 text-center">
              <div
                className={`text-xs font-medium ${
                  isStepCurrent(step.status) ? "text-blue-600" : "text-gray-500"
                }`}
              >
                {step.label}
              </div>
              {step.date && (
                <div className="text-xs text-gray-400 mt-1">
                  {new Date(step.date).toLocaleDateString()}
                </div>
              )}
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
