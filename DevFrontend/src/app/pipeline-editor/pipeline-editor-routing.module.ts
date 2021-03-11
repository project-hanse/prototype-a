import {NgModule} from '@angular/core';
import {Routes, RouterModule} from '@angular/router';
import {ViewPipelineComponent} from './view-pipeline/view-pipeline.component';

const routes: Routes = [
  {
    path: 'view/:id',
    component: ViewPipelineComponent
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class PipelineEditorRoutingModule {
}
