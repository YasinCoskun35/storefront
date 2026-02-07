# Frontend Debugging Guide - Storefront Next.js App

## 🎯 Quick Start Debugging Methods

### Method 1: Browser DevTools (Fastest)

#### Open DevTools:
- **Chrome/Edge**: `F12` or `Ctrl+Shift+I` (Windows) / `Cmd+Option+I` (Mac)
- **Firefox**: `F12` or `Ctrl+Shift+I` (Windows) / `Cmd+Option+I` (Mac)

#### Key Features:
```javascript
// Console tab - Add these to your code:
console.log('Variable value:', myVariable);
console.error('Error:', error);
console.warn('Warning:', data);
console.table(arrayData); // Shows arrays/objects as table
console.group('Group Name'); // Group related logs
console.groupEnd();

// Debugger statement - Pauses execution
debugger; // Browser will stop here when DevTools is open
```

---

### Method 2: VS Code Debugging (Recommended)

#### Setup (Already Created ✅):
The `.vscode/launch.json` file is ready!

#### How to Use:

1. **Open VS Code** in the `web` folder
2. **Set Breakpoints**: Click to the left of line numbers
3. **Start Debugging**: Press `F5` or go to Run → Start Debugging
4. **Choose Configuration**:
   - **Next.js: debug server-side** - For Server Components
   - **Next.js: debug client-side** - For Client Components (browser)
   - **Next.js: debug full stack** - Both at once

#### Debug Controls:
- `F5` - Continue
- `F10` - Step Over
- `F11` - Step Into
- `Shift+F11` - Step Out
- `Ctrl+Shift+F5` - Restart
- `Shift+F5` - Stop

---

### Method 3: React DevTools (Component Debugging)

#### Install Extension:
- **Chrome**: [React Developer Tools](https://chrome.google.com/webstore/detail/react-developer-tools/fmkadmapgofadopljbjfkapdkoienihi)
- **Firefox**: [React Developer Tools](https://addons.mozilla.org/en-US/firefox/addon/react-devtools/)

#### Features:
1. **Components Tab**: Inspect React component tree
2. **Profiler Tab**: Measure component performance
3. **Props Inspector**: View/edit component props
4. **State Inspector**: View/edit component state
5. **Hooks Inspector**: Debug custom hooks

---

## 🔍 Debugging Specific Issues

### Debug API Calls

#### In Browser DevTools:
1. Open **Network Tab** (`F12` → Network)
2. Filter by **Fetch/XHR**
3. Click on request to see:
   - Request Headers
   - Request Payload
   - Response Data
   - Status Code

#### In Code:
```typescript
// Add interceptor to axios (src/lib/api.ts)
api.interceptors.request.use(request => {
  console.log('🚀 Request:', request.method?.toUpperCase(), request.url);
  console.log('📦 Data:', request.data);
  return request;
});

api.interceptors.response.use(
  response => {
    console.log('✅ Response:', response.status, response.config.url);
    console.log('📥 Data:', response.data);
    return response;
  },
  error => {
    console.error('❌ Error:', error.response?.status, error.config?.url);
    console.error('📥 Error Data:', error.response?.data);
    return Promise.reject(error);
  }
);
```

---

### Debug Authentication Issues

```typescript
// Check auth state
console.log('Auth State:', {
  isAuthenticated: useAuth().isAuthenticated,
  user: useAuth().user,
  token: localStorage.getItem('accessToken')
});

// Debug login flow
const login = async (email, password) => {
  console.log('🔐 Login attempt:', { email });
  
  try {
    const response = await authApi.login(email, password);
    console.log('✅ Login success:', response);
  } catch (error) {
    console.error('❌ Login failed:', error);
  }
};
```

---

### Debug React Query

```typescript
import { useQuery } from '@tanstack/react-query';

const { data, error, isLoading, refetch } = useQuery({
  queryKey: ['products'],
  queryFn: fetchProducts
});

// Debug query state
console.log('Query State:', {
  data,
  error,
  isLoading,
  status: query.status
});
```

---

### Debug Form Validation (react-hook-form)

```typescript
import { useForm } from 'react-hook-form';

const { 
  formState: { errors, isValid, isDirty },
  watch,
  getValues
} = useForm();

// Watch all form values
console.log('Form Values:', watch());

// Log errors
console.log('Form Errors:', errors);

// Debug on submit
const onSubmit = (data) => {
  console.log('📝 Form submitted:', data);
  console.log('🔍 All values:', getValues());
};
```

---

## 🛠️ Common Debugging Scenarios

### 1. Component Not Rendering

```typescript
'use client'; // Make sure this is present for Client Components

export default function MyComponent() {
  console.log('🎨 Component rendering');
  
  useEffect(() => {
    console.log('🚀 Component mounted');
    return () => console.log('💀 Component unmounted');
  }, []);
  
  return <div>Content</div>;
}
```

### 2. State Not Updating

```typescript
const [count, setCount] = useState(0);

const increment = () => {
  console.log('Before:', count);
  setCount(prev => {
    console.log('Setting:', prev + 1);
    return prev + 1;
  });
  console.log('After (still old):', count); // State updates are async!
};

useEffect(() => {
  console.log('Count changed to:', count); // Use effect to see new value
}, [count]);
```

### 3. Infinite Re-renders

```typescript
// ❌ Bad - Causes infinite loop
useEffect(() => {
  setData(newData);
}); // No dependency array!

// ✅ Good
useEffect(() => {
  setData(newData);
}, []); // Empty array = run once on mount

// ✅ Good with dependency
useEffect(() => {
  setData(newData);
}, [someValue]); // Run when someValue changes
```

### 4. Props Not Passing

```typescript
// Parent
console.log('Passing props:', { name, age });
<ChildComponent name={name} age={age} />

// Child
export default function ChildComponent({ name, age }) {
  console.log('Received props:', { name, age });
  return <div>{name} - {age}</div>;
}
```

---

## 🔬 Advanced Debugging

### Performance Profiling

```typescript
import { Profiler } from 'react';

function onRenderCallback(
  id, // the "id" prop of the Profiler tree
  phase, // "mount" or "update"
  actualDuration, // time spent rendering
  baseDuration, // estimated time to render without memoization
  startTime, // when React began rendering
  commitTime // when React committed this update
) {
  console.log('Profiler:', {
    id,
    phase,
    actualDuration,
    baseDuration
  });
}

<Profiler id="MyComponent" onRender={onRenderCallback}>
  <MyComponent />
</Profiler>
```

### Custom Debug Hook

```typescript
// hooks/useDebug.ts
export function useDebug(componentName: string, props: any) {
  useEffect(() => {
    console.log(`[${componentName}] Mounted`);
    return () => console.log(`[${componentName}] Unmounted`);
  }, [componentName]);

  useEffect(() => {
    console.log(`[${componentName}] Props changed:`, props);
  }, [componentName, props]);
}

// Usage
function MyComponent(props) {
  useDebug('MyComponent', props);
  // ... rest of component
}
```

### Network Debugging

```typescript
// See all fetch requests
window.addEventListener('beforeunload', () => {
  console.log('📊 Performance Entries:', 
    performance.getEntriesByType('resource')
  );
});

// Monitor specific endpoint
const originalFetch = window.fetch;
window.fetch = function(...args) {
  console.log('🌐 Fetch:', args[0]);
  return originalFetch.apply(this, args);
};
```

---

## 🎓 Debugging Best Practices

### 1. Use Descriptive Console Logs

```typescript
// ❌ Bad
console.log(data);

// ✅ Good
console.log('📦 Products loaded:', data);
console.log('🔍 Filtering products by category:', category, filteredProducts);
```

### 2. Conditional Debugging

```typescript
const DEBUG = process.env.NODE_ENV === 'development';

function debug(...args: any[]) {
  if (DEBUG) {
    console.log(...args);
  }
}

// Usage
debug('🔍 User state:', user);
```

### 3. Group Related Logs

```typescript
console.group('🔐 Authentication Flow');
console.log('1. Validating credentials');
console.log('2. Sending login request');
console.log('3. Storing token');
console.groupEnd();
```

### 4. Use Error Boundaries

```typescript
// components/ErrorBoundary.tsx
'use client';

import { Component, ReactNode } from 'react';

interface Props {
  children: ReactNode;
}

interface State {
  hasError: boolean;
  error?: Error;
}

export class ErrorBoundary extends Component<Props, State> {
  constructor(props: Props) {
    super(props);
    this.state = { hasError: false };
  }

  static getDerivedStateFromError(error: Error) {
    console.error('🚨 Error Boundary caught:', error);
    return { hasError: true, error };
  }

  componentDidCatch(error: Error, errorInfo: any) {
    console.error('Error details:', { error, errorInfo });
  }

  render() {
    if (this.state.hasError) {
      return (
        <div className="p-4 bg-red-50 border border-red-200 rounded">
          <h2 className="text-red-800 font-bold">Something went wrong</h2>
          <pre className="text-sm mt-2">{this.state.error?.message}</pre>
        </div>
      );
    }

    return this.props.children;
  }
}
```

---

## 🐞 Common Issues & Solutions

### Issue: "Hydration Error"

**Cause**: Server and client render different content

**Debug:**
```typescript
// Check if running on client
const [isClient, setIsClient] = useState(false);

useEffect(() => {
  setIsClient(true);
}, []);

if (!isClient) return null; // or return loading state
```

### Issue: "Cannot read property of undefined"

**Debug:**
```typescript
// Add optional chaining
console.log(user?.name);

// Use nullish coalescing
const name = user?.name ?? 'Guest';

// Add type guards
if (user && user.name) {
  console.log(user.name);
}
```

### Issue: API calls not working

**Debug:**
```typescript
// Check in browser DevTools Network tab
// 1. Is request being sent?
// 2. What's the status code?
// 3. What's the response?

// Add detailed logging
try {
  console.log('📡 Calling API:', endpoint);
  const response = await api.get(endpoint);
  console.log('✅ Success:', response.data);
  return response.data;
} catch (error) {
  console.error('❌ API Error:', {
    message: error.message,
    status: error.response?.status,
    data: error.response?.data
  });
  throw error;
}
```

---

## 🚀 Quick Debug Shortcuts

### Browser Console Shortcuts:
- `$_` - Last evaluated expression
- `$0` - Currently selected DOM element
- `$('selector')` - Query selector (like jQuery)
- `$$('selector')` - Query selector all
- `copy(object)` - Copy to clipboard
- `clear()` - Clear console

### VS Code Shortcuts:
- `F5` - Start/Continue debugging
- `Shift+F5` - Stop debugging
- `F9` - Toggle breakpoint
- `F10` - Step over
- `F11` - Step into
- `Ctrl+Shift+F5` - Restart debugging

---

## 📊 Debugging Tools Summary

| Tool | Best For | How to Access |
|------|----------|---------------|
| Browser DevTools | Quick debugging, network inspection | `F12` |
| VS Code Debugger | Breakpoints, step-through debugging | `F5` in VS Code |
| React DevTools | Component inspection, props/state | Browser extension |
| Network Tab | API calls, request/response | DevTools → Network |
| Console Logs | Quick checks, tracing flow | `console.log()` |
| React Query DevTools | Query state, cache inspection | Add to app |

---

## 🎯 Next Steps

1. **Install React DevTools** browser extension
2. **Try VS Code debugging** - Press `F5` in the `web` folder
3. **Add console.log statements** where you need to debug
4. **Use Network tab** to inspect API calls
5. **Set breakpoints** in VS Code to step through code

**Happy Debugging!** 🐛🔍



