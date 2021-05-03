import {BasePersistentModel} from './base-persistent-model';
import {PipelineNode} from './pipelineNode';

export interface Pipeline extends BasePersistentModel {
  name: string;
  root: PipelineNode[];
}
