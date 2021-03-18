import {BasePersistentModel} from './base-persistent-model';
import {Block} from './block';

export interface Pipeline extends BasePersistentModel {
  name: string;
  root: Block[];
}
