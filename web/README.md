# Storefront Web - Next.js 15 Frontend

Public-facing storefront built with Next.js 15, React 19, and TypeScript.

## Tech Stack

- **Next.js 15** (App Router)
- **React 19**
- **TypeScript**
- **Tailwind CSS** for styling
- **shadcn/ui** components
- **TanStack Query v5** for client-side state management
- **Axios** for API requests

## Getting Started

### Installation

```bash
# Install dependencies
npm install

# Or use the provided command
npm install @tanstack/react-query axios clsx tailwind-merge lucide-react
```

### Development

```bash
# Run the development server
npm run dev
```

Open [http://localhost:3000](http://localhost:3000) with your browser.

### Environment Variables

Create a `.env.local` file:

```
API_URL=http://localhost:8080
```

For production with Docker:
```
API_URL=http://api:8080
```

## Features

### Implemented

- ✅ **Product Catalog** with filtering, search, and pagination
- ✅ **Product Detail Pages** with SEO metadata and image galleries
- ✅ **Blog Listing** with pagination
- ✅ **Static Pages** (About, Contact)
- ✅ **Responsive Navigation** with global search
- ✅ **Server Components** for initial data fetching (SEO-friendly)
- ✅ **Client Components** for interactive features
- ✅ **Image Optimization** with next/image
- ✅ **Auth API Proxy** for HttpOnly cookies

### Architecture

**SSR/CSR Handling:**
- Server components fetch initial data for SEO
- Client components handle user interactions (filtering, search)
- API client automatically detects server/client context

**API Base URL:**
- Server-side: Uses `API_URL` env variable (defaults to localhost:8080)
- Client-side: Uses relative paths to localhost:8080

**Image URLs:**
- Configured in `next.config.ts` to allow images from `localhost:8080`
- Supports both `/uploads/**` paths from API

## Project Structure

```
web/
├── src/
│   ├── app/                    # Next.js App Router
│   │   ├── (public)/          # Public route group (future)
│   │   ├── products/          # Product pages
│   │   ├── blog/              # Blog pages
│   │   ├── api/auth/          # Auth proxy routes
│   │   └── layout.tsx         # Root layout
│   ├── components/
│   │   ├── layout/            # Header, Footer
│   │   ├── products/          # Product components
│   │   ├── providers/         # React Query provider
│   │   └── ui/                # shadcn/ui components
│   └── lib/
│       ├── api.ts             # Typed API client
│       └── utils.ts           # Utility functions
├── next.config.ts
├── tailwind.config.ts
└── tsconfig.json
```

## API Integration

The frontend communicates with the .NET backend API:

**Products:**
- `GET /api/catalog/products` - Search and filter products
- `GET /api/catalog/products/{id}` - Product details
- `GET /api/catalog/categories` - Category list

**Content:**
- `GET /api/content/blog` - Blog posts
- `GET /api/content/pages/{slug}` - Static pages

**Authentication:**
- `POST /api/auth/login` - Proxied through Next.js API route

## Development Notes

### shadcn/ui Components

Basic implementations of Button, Card, and Input are included. For full shadcn/ui:

```bash
npx shadcn@latest init
npx shadcn@latest add button card input
```

### TypeScript

All components are fully typed with TypeScript for better DX.

### Performance

- Server components for initial data (SEO + fast initial load)
- Client components only where needed (filters, interactive UI)
- Image optimization with next/image
- TanStack Query for efficient data fetching and caching

## Building for Production

```bash
npm run build
npm run start
```

## Docker Integration (Future)

The frontend will be containerized and served via Nginx reverse proxy:
- `web` container for Next.js
- `nginx` for SSL termination and routing
- API at `/api/*` proxied to backend
- Static assets served directly

## License

MIT

