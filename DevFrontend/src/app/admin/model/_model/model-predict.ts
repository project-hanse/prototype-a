import {DatasetType} from '../../../pipeline/_model/dataset';

export interface ModelPredict {
	input_0_model_type?: string;
	input_0_dataset_type?: DatasetType;
	input_1_model_type?: string;
	input_1_dataset_type?: DatasetType;
	input_2_model_type?: string;
	input_2_dataset_type?: DatasetType;
	feat_pred_id?: string;
	feat_pred_count?: number;
}
