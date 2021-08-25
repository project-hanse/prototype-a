import {NgModule} from '@angular/core';
import {RouterModule, Routes} from '@angular/router';
import {StatusComponent} from './status/status.component';

const routes: Routes = [
  {
    path: 'status',
    component: StatusComponent
  },
  {
    path: '',
    redirectTo: 'status'
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class DevToolsRoutingModule {
}
