import {NgModule} from '@angular/core';
import {Routes, RouterModule} from '@angular/router';

const routes: Routes = [
  {
    path: 'pipeline',
    loadChildren: () => import('./pipeline/pipeline.module').then(m => m.PipelineModule)
  },
  {
    path: 'dev',
    loadChildren: () => import('./dev-tools/dev-tools.module').then(m => m.DevToolsModule)
  },
  {
    path: '',
    redirectTo: 'pipeline'
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule {
}
