# TESTS

## Purpose of Tests
### Hooks
- useMutationWithToast: success/error toast notifications, callbacks with queryClient, error logging
- usePagination: page changes, page size changes with reset, custom initialization, resetToFirstPage
- useUnsavedChanges: React Router navigation blocking (useBlocker), browser beforeunload event, proceed/reset functions, edge cases
### Components
- FormShell: render props pattern, validation logic, form submission, field updates
- ProductForm: form modes (edit/view/try), create/update mutations, business validation rules
- ProtectedRoute: loading states, unauthenticated redirects, authenticated access, state transitions
- AdminProductList: RBAC - button visibility based on user roles and demo products
- FormChipInput: comprehensive autocomplete chip input testing (rendering, adding chips, removing chips, autocomplete, keyboard navication, ARIA attributes, disabled state, click to focus)

## Test Overview
### Hooks
- useMutationWithToast: `src/hooks/utilities/__tests__/useMutationWithToast.test.ts` (5 tests)
- usePagination: `src/hooks/utilities/__tests__/usePagination.test.ts` (5 tests)
- useUnsavedChanges: `src/hooks/utilities/__tests__/useUnsavedChanges.test.ts` (14 tests)
### Components
- FormShell: `src/components/primitives/__tests__/FormShell.test.tsx` (6 tests)
- ProductForm: `src/components/admin/__tests__/ProductForm.test.tsx` (6 tests)
- ProtectedRoute: `src/components/auth/__tests__/ProtectedRoute.test.tsx` (6 tests)
- AdminProductList: `src/components/admin/__tests__/AdminProductList.test.tsx` (4 tests)
- FormChipInput: `src/components/primitives/__tests__/FormChipInput.test.tsx` (47 tests)

## Run tests
```bash
npm test              # watch mode
npm run test:run      # single run
npm run test:ui       # browser UI
```

## Future
- E2E tests with Playwright (login flow, product creation)