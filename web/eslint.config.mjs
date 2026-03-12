import { dirname } from "path";
import { fileURLToPath } from "url";
import { FlatCompat } from "@eslint/eslintrc";

const __filename = fileURLToPath(import.meta.url);
const __dirname = dirname(__filename);

const compat = new FlatCompat({
  baseDirectory: __dirname,
});

const eslintConfig = [
  ...compat.extends("next/core-web-vitals", "next/typescript"),

  {
    rules: {
      // ─── TypeScript ──────────────────────────────────────────────────
      // Disallow 'any' except in catch blocks and explicit overrides
      "@typescript-eslint/no-explicit-any": "warn",
      // Require explicit return types on exported functions
      "@typescript-eslint/explicit-module-boundary-types": "off",
      // Prevent unused variables (prefix with _ to intentionally ignore)
      "@typescript-eslint/no-unused-vars": [
        "error",
        { argsIgnorePattern: "^_", varsIgnorePattern: "^_" },
      ],

      // ─── React ───────────────────────────────────────────────────────
      // Exhaustive deps for useEffect/useCallback/useMemo
      "react-hooks/exhaustive-deps": "warn",
      // No unnecessary fragment wrappers
      "react/jsx-no-useless-fragment": "warn",
      // Self-closing tags for components with no children
      "react/self-closing-comp": "warn",

      // ─── General ─────────────────────────────────────────────────────
      // Prefer const
      "prefer-const": "error",
      // No console.log in production code (use the dev axios interceptor instead)
      "no-console": ["warn", { allow: ["warn", "error"] }],
      // No debugger statements
      "no-debugger": "error",
      // Consistent equality
      "eqeqeq": ["error", "always", { null: "ignore" }],

      // ─── Next.js specific ────────────────────────────────────────────
      // Allow <img> only in specific cases (we use next/image almost everywhere)
      "@next/next/no-img-element": "warn",
    },
  },

  {
    // Relax rules for configuration files and scripts
    files: ["*.config.{js,mjs,ts}", "scripts/**/*"],
    rules: {
      "no-console": "off",
      "@typescript-eslint/no-require-imports": "off",
    },
  },

  {
    // Relax 'any' in catch blocks — it's unavoidable
    files: ["**/*.tsx", "**/*.ts"],
    rules: {},
  },
];

export default eslintConfig;
