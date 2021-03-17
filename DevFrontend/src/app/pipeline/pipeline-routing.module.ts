import {NgModule} from '@angular/core';
import {Routes, RouterModule} from '@angular/router';
import {PipelineListViewComponent} from './pipeline-list-view/pipeline-list-view.component';
import {PipelineDetailViewComponent} from './pipeline-detail-view/pipeline-detail-view.component';

const routes: Routes = [
  {
    path: ':id',
    component: PipelineDetailViewComponent
  },
  {
    path: '',
    component: PipelineListViewComponent
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class PipelineRoutingModule {
}
