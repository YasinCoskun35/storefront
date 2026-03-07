/**
 * Example: Auto-generating slug from product name in a form
 * 
 * This shows how to use the generateSlug utility in a React form
 * with automatic Turkish character handling
 */

'use client';

import { useState, useEffect } from 'react';
import { generateSlug } from '@/lib/utils';

export function ProductFormExample() {
  const [productName, setProductName] = useState('');
  const [slug, setSlug] = useState('');
  const [isSlugManuallyEdited, setIsSlugManuallyEdited] = useState(false);

  // Auto-generate slug when product name changes
  useEffect(() => {
    if (!isSlugManuallyEdited && productName) {
      setSlug(generateSlug(productName));
    }
  }, [productName, isSlugManuallyEdited]);

  return (
    <div className="space-y-4">
      {/* Product Name Input */}
      <div>
        <label htmlFor="name" className="block text-sm font-medium mb-1">
          Ürün Adı / Product Name
        </label>
        <input
          id="name"
          type="text"
          value={productName}
          onChange={(e) => setProductName(e.target.value)}
          placeholder="örn: Özel Tasarım Koltuk"
          className="w-full px-3 py-2 border rounded-md"
        />
      </div>

      {/* Auto-generated Slug (can be edited) */}
      <div>
        <label htmlFor="slug" className="block text-sm font-medium mb-1">
          URL Slug (Otomatik / Auto-generated)
        </label>
        <input
          id="slug"
          type="text"
          value={slug}
          onChange={(e) => {
            setSlug(e.target.value);
            setIsSlugManuallyEdited(true);
          }}
          placeholder="ozel-tasarim-koltuk"
          className="w-full px-3 py-2 border rounded-md font-mono text-sm"
        />
        <p className="mt-1 text-xs text-gray-500">
          Türkçe karakterler otomatik olarak dönüştürülür / Turkish characters are automatically converted
        </p>
      </div>

      {/* Preview URL */}
      {slug && (
        <div className="p-3 bg-gray-50 rounded-md">
          <p className="text-xs text-gray-600 mb-1">URL Önizleme / URL Preview:</p>
          <p className="font-mono text-sm text-blue-600">
            https://example.com/products/{slug}
          </p>
        </div>
      )}
    </div>
  );
}

/**
 * Example Usage in Admin Product Form:
 * 
 * import { generateSlug } from '@/lib/utils';
 * 
 * const handleSubmit = async (data: ProductFormData) => {
 *   const payload = {
 *     name: data.name,
 *     // Backend will auto-generate, but you can send it too
 *     slug: generateSlug(data.name),
 *     // ... other fields
 *   };
 *   
 *   await api.post('/api/catalog/products', payload);
 * };
 */

/**
 * Example Turkish Product Names:
 * 
 * Input: "Çok Rahat Koltuk"
 * Slug:  "cok-rahat-koltuk"
 * 
 * Input: "Özel Döşeme İşçiliği"
 * Slug:  "ozel-doseme-isciligi"
 * 
 * Input: "İki Kişilik Şezlong"
 * Slug:  "iki-kisilik-sezlong"
 * 
 * Input: "Ağaç & Deri Kanepe"
 * Slug:  "agac-and-deri-kanepe"
 */
