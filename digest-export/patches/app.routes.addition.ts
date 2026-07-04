// ============================================================
// PATCH: app.routes.ts
// Add this import at the top of your routes file:
// ============================================================

import { DigestComponent } from './digest/digest.component';

// ============================================================
// Then add this entry inside the routes array:
// ============================================================

{
  path: 'digest',
  component: DigestComponent,
  canActivate: [authGuard]   // remove canActivate if you don't use an auth guard
},
