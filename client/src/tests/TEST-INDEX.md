# TESTS

## Purpose of Tests
- FormShell: render props pattern, validation logic, form submission, field updates
- useMutationWithToast: success/error toast notifications, callbacks with queryClient, error logging
- ProductForm: form modes (edit/view/try), create/update mutations, business validation rules
- ProtectedRoute: loading states, unauthenticated redirects, authenticated access, state transitions
- AdminProductList: RBAC - button visibility based on user roles and demo products

## Test Overview
- FormShell: `src/components/primitives/__tests__/FormShell.test.tsx` (5 tests)
- useMutationWithToast: `src/hooks/utilities/__tests__/useMutationWithToast.test.ts` (5 tests)
- ProductForm: `src/components/admin/__tests__/ProductForm.test.tsx` (6 tests)
- ProtectedRoute: `src/components/auth/__tests__/ProtectedRoute.test.tsx` (6 tests)
- AdminProductList: `src/components/admin/__tests__/AdminProductList.test.tsx` (4 tests)

## Run tests
```bash
npm test              # watch mode
npm run test:run      # single run
npm run test:ui       # browser UI
```

## Future
- FormChipInput tests (keyboard navigation with arrows/enter/escape/backspace)
- E2E tests with Playwright (login flow, product creation)