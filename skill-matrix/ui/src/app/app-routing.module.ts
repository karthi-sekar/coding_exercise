import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { DashboardComponent } from './dashboard/dashboard.component';
import { ManageComponent } from './manage/manage.component';
import { CareerplanComponent } from './careerplan/careerplan.component';
import { WhatsnewComponent } from './whatsnew/whatsnew.component';


const routes: Routes = [
  {
    path: '',
    redirectTo: 'dashboard',
    pathMatch: 'full',
  },
  {
    path: 'dashboard',
    component: DashboardComponent,
    data: {
      title: 'Dashboard'
    }
  },
  {
    path: 'manage',
    component: ManageComponent,
    data: {
      title: 'Manage Skills'
    }
  },
  {
    path: 'careerplan',
    component: CareerplanComponent,
    data: {
      title: 'Career Plan'
    }
  },
  {
    path: 'whatsnew',
    component: WhatsnewComponent,
    data: {
      title: 'What&quot;s New' 
    }
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
