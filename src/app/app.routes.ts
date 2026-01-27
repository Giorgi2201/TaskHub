import { Routes } from '@angular/router';
import { RequestListComponent } from './request-list/request-list.component';
import { RequestDetailComponent } from './request-detail/request-detail.component';
import { RequestNewComponent } from './request-new/request-new.component';
import { RequestFormComponent } from './request-form/request-form.component';
import { AdminComponent } from './admin/admin.component';
import { LoginComponent } from './login/login.component';
import { DashboardComponent } from './dashboard/dashboard.component';
import { NewsComponent } from './news/news.component';
import { InfoComponent } from './info/info.component';
import { authGuard } from './auth.guard';

export const routes: Routes = [
  {
    path: '',
    redirectTo: '/login',
    pathMatch: 'full'
  },
  {
    path: 'login',
    component: LoginComponent
  },
  {
    path: 'dashboard',
    component: DashboardComponent,
    canActivate: [authGuard]
  },
  {
    path: 'requests',
    component: RequestListComponent,
    canActivate: [authGuard]
  },
  {
    path: 'request/new',
    component: RequestNewComponent,
    canActivate: [authGuard]
  },
  {
    path: 'request/new/:category',
    component: RequestFormComponent,
    canActivate: [authGuard]
  },
  {
    path: 'request/:id',
    component: RequestDetailComponent,
    canActivate: [authGuard]
  },
  {
    path: 'news',
    component: NewsComponent,
    canActivate: [authGuard]
  },
  {
    path: 'info',
    component: InfoComponent,
    canActivate: [authGuard]
  },
  {
    path: 'admin',
    component: AdminComponent,
    canActivate: [authGuard]
  }
];
