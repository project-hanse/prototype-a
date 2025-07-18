import logging
from typing import Callable

import pandas as pd

from src.operations.operations_data_converter import OperationsDataConverter
from src.operations.operations_file_input import OperationsFileInputCollection
from src.operations.operations_openml import OperationsOpenML
from src.operations.operations_plots_matplotlib import PlotsMatPlotLib
from src.operations.operations_prophet import OperationsProphet
from src.operations.operations_single_input_pd_custom import OperationsSingleInputPandasCustom
from src.operations.operations_single_input_pd_wrappers import OperationsSingleInputPandasWrappers
from src.operations.operations_sklearn_classifiers import OperationsSklearnClassifiers
from src.operations.operations_sklearn_custom import OperationsSklearnCustom
from src.operations.operations_sklearn_dict_vectorizer import OperationsSklearnDictVectorizer
from src.operations.operations_sklearn_preprocessing_wrappers import OperationsSklearnPreprocessingWrappers


class OperationService:

	@staticmethod
	def operation(dataset: pd.DataFrame) -> pd.DataFrame:
		print("dataset:", dataset)
		return dataset

	def __init__(self, logger: logging) -> None:
		self.logger = logger
		self.local_operations = {}
		super().__init__()

	def init(self):
		self.logger.info("Initializing local operations store...")

		self.local_operations[
			"dfbca055-69f1-40df-9639-023ec6363bac"] = OperationsFileInputCollection.pd_file_input_read_csv
		self.local_operations[
			"2413f0d5-c3c0-4ce6-b1f3-5837b296ab92"] = OperationsFileInputCollection.pd_file_input_read_excel

		# Single Input Pandas
		self.local_operations[
			"0ebc4dd5-6a81-48e7-8abd-3488c608020f"] = OperationsSingleInputPandasWrappers.pd_single_input_transpose
		self.local_operations[
			"0759dede-2cee-433c-b314-10a8fa456e62"] = OperationsSingleInputPandasWrappers.pd_single_input_generic
		self.local_operations[
			"de26c7a0-0444-414d-826f-458cd3b8979c"] = OperationsSingleInputPandasWrappers.pd_single_input_set_index
		self.local_operations[
			"e44cc87e-3150-4387-b5dc-f7a7b8131d87"] = OperationsSingleInputPandasWrappers.pd_single_input_reset_index
		self.local_operations[
			"0fb2b572-bc3c-48d5-9c31-6bf0d0f7cc61"] = OperationsSingleInputPandasWrappers.pd_single_input_rename
		self.local_operations[
			"43f6b64a-ae47-45e3-95e5-55dc65d4249e"] = OperationsSingleInputPandasWrappers.pd_single_input_drop
		self.local_operations[
			"074669e8-9b60-48ce-bfc9-509d5990f517"] = OperationsSingleInputPandasWrappers.pd_single_input_mean
		self.local_operations[
			"7b0bb47f-f997-43d8-acb1-c31f2a22475d"] = OperationsSingleInputPandasWrappers.pd_single_input_select_columns
		self.local_operations[
			"d2701fa4-b038-4fcb-b981-49f9f123da01"] = OperationsSingleInputPandasWrappers.pd_single_input_select_rows
		self.local_operations[
			"5c9b34fc-ac4f-4290-9dfe-418647509559"] = OperationsSingleInputPandasWrappers.pd_single_input_trim_rows
		self.local_operations[
			"db8b6a9d-d01f-4328-b971-fa56ac350320"] = OperationsSingleInputPandasWrappers.pd_single_input_make_row_header
		self.local_operations[
			"7537069e-03b2-481c-b6a3-fca096e4acf8"] = OperationsSingleInputPandasWrappers.pd_single_input_sort_index
		self.local_operations[
			"d249e0be-abc4-4801-8622-4e39b4be49bf"] = OperationsSingleInputPandasWrappers.pd_single_input_replace
		self.local_operations[
			"f2abca86-2175-4d44-8a26-c7bd68ee2dc6"] = OperationsSingleInputPandasWrappers.pd_single_input_interpolate

		# Double Input Pandas
		self.local_operations[
			"9acea312-713e-4de8-b8db-5d33613ab2f1"] = OperationsSingleInputPandasWrappers.pd_double_input_join
		self.local_operations[
			"804aadc7-4f9e-41cc-8ccc-e386459fbc63"] = OperationsSingleInputPandasWrappers.pd_double_input_concat

		# SkLearn Operations
		self.local_operations["2878ca36-8e03-4825-8afa-552064686337"] = OperationsSklearnCustom.sklearn_double_input_predict
		self.local_operations["ed0beb29-a853-4421-ba3e-eb2bdc050117"] = OperationsSklearnCustom.sklearn_split
		self.local_operations["fa66798e-81d4-4656-a92f-00e84a2f8436"] = OperationsSklearnCustom.sklearn_split_v2
		self.local_operations["c3ed2e34-0f7b-492c-9152-e3d5cf3ebeb0"] = OperationsSklearnCustom.sklearn_transform
		self.local_operations["95cd82c3-01cf-488c-b156-22e06993b2f3"] = OperationsSklearnCustom.sklearn_extract_features
		self.local_operations["34c12968-f79b-43b8-a934-39ce4e2e763d"] = OperationsSklearnCustom.sklearn_extract_targets
		self.local_operations[
			"36564f26-f147-47f1-95fb-884dba993494"] = OperationsSklearnClassifiers.sklearn_create_fit_classifier_svc
		self.local_operations[
			"4923d3da-19a7-45da-9fbc-9a0737d6f64b"] = OperationsSklearnClassifiers.sklearn_create_fit_classifier_linear_svc
		self.local_operations[
			"24f097f7-08db-4e78-a7dd-b510bfa4852e"] = OperationsSklearnClassifiers.sklearn_create_fit_classifier_naive_bayes_gaussian
		self.local_operations[
			"e34a64ab-2c1e-4498-a9e3-40f5fe78fa5e"] = OperationsSklearnClassifiers.sklearn_create_fit_classifier_k_neighbors
		self.local_operations[
			"a5918ebc-5ba2-461a-8b5f-4215328d957e"] = OperationsSklearnClassifiers.sklearn_create_fit_classifier_random_forest
		self.local_operations[
			"93d21ed8-acc5-4a67-8678-93a705a0878b"] = OperationsSklearnClassifiers.sklearn_create_fit_classifier_decision_tree
		self.local_operations[
			"067c7cd4-87f6-43e2-a733-26e5c51ef875"] = OperationsSklearnClassifiers.sklearn_classifier_score
		self.local_operations[
			"ca3a3e8e-9618-4450-bab7-c0a2d6cc48ba"] = OperationsSklearnClassifiers.sklearn_classifier_predict_join
		self.local_operations[
			"46ae1799-92f0-4337-b06b-de2c981f4ed9"] = OperationsSklearnClassifiers.sklearn_classifier_predict

		# Data Converter
		self.local_operations["939e96e1-76e3-4a50-a484-472690d8fd6c"] = OperationsDataConverter.data_converter_df_to_dict
		self.local_operations["ec5aaf96-b940-41fd-b1b1-f55d933282df"] = OperationsDataConverter.data_converter_np_array_to_df

		# SkLearn DictVectorizer
		self.local_operations[
			"53972402-11e2-46db-98fd-ca3c0481081d"] = OperationsSklearnDictVectorizer.sklearn_dict_vectorizer
		self.local_operations[
			"5a193830-9c94-4141-963e-02b1c835174b"] = OperationsSklearnDictVectorizer.sklearn_dict_vectorizer_fit
		self.local_operations[
			"ab3a7503-1bc6-4a40-9120-a59d0e31055f"] = OperationsSklearnDictVectorizer.sklearn_dict_vectorizer_fit_transform
		self.local_operations[
			"383d001f-26f4-4d31-8ce3-59ff2655f711"] = OperationsSklearnDictVectorizer.sklearn_dict_vectorizer_transform
		self.local_operations[
			"e9c6d795-57c1-40f5-ad50-9cda4fa170de"] = OperationsSklearnDictVectorizer.sklearn_dict_vectorizer_inverse_transform

		# SkLearn Preprocessing
		self.local_operations[
			"f3fc1084-4c1a-495d-846c-013f6f37985c"] = OperationsSklearnPreprocessingWrappers.sklearn_label_encoder
		self.local_operations[
			"d90eb613-335d-416d-a4a8-91bcd6aa57aa"] = OperationsSklearnPreprocessingWrappers.sklearn_one_hot_encoder
		self.local_operations[
			"49d1775b-b61d-4dcf-afbf-45a561c580ad"] = OperationsSklearnPreprocessingWrappers.sklearn_encoder_fit
		self.local_operations[
			"07af5ae2-7785-4bf4-8cee-8622d3281f5f"] = OperationsSklearnPreprocessingWrappers.sklearn_encoder_transform
		self.local_operations[
			"3ed065e7-dcbb-4400-aa9e-38bc6dd21f58"] = OperationsSklearnPreprocessingWrappers.sklearn_encoder_fit_transform
		self.local_operations[
			"9eedb02b-f2de-4354-8be7-565e90c90907"] = OperationsSklearnPreprocessingWrappers.sklearn_encoder_inverse_transform

		# Custom Operations
		self.local_operations[
			"d424052c-caa5-43b2-a9bc-d543167b983f"] = OperationsSingleInputPandasCustom.pd_single_input_set_date_index
		self.local_operations[
			"e8877645-b0b6-43c9-84ed-79c6565b6f28"] = OperationsSingleInputPandasCustom.pd_single_input_df_to_numeric
		self.local_operations[
			"0fc78290-88c8-49b0-878e-a25a3f6452c1"] = OperationsSingleInputPandasCustom.pd_single_input_resample
		self.local_operations[
			"1991ccdb-c8ba-4a71-a325-420e48471379"] = OperationsSingleInputPandasCustom.pd_single_input_groupby
		self.local_operations[
			"0b60e908-fae2-4d33-aa81-5d1fdc706c12"] = PlotsMatPlotLib.matplot_plot_pd
		self.local_operations["9c876745-9d61-4b0b-a32a-2de523b44d0b"] = OperationsOpenML.load_data_from_openml

		# Prophet Operations
		self.local_operations["c01f8b5a-3c71-466a-b2ab-2abb2aa105ba"] = OperationsProphet.prophet_fit
		self.local_operations["df7bff17-f175-4db5-9b04-955d6f261380"] = OperationsProphet.prophet_make_future_dataframe
		self.local_operations["e358a55d-ba47-4d14-93b7-cf52fd29c64d"] = OperationsProphet.prophet_predict

	def get_operation_by_id(self, operation_id: str) -> Callable:
		self.logger.info('Getting operation %s' % operation_id)
		if operation_id in self.local_operations:
			return self.local_operations[operation_id]

		self.logger.warn("Operation with id %s not found" % operation_id)
		raise NotImplementedError("Operation with this id is not implemented")

	def get_file_operation_by_id(self, operation_id) -> Callable:
		self.logger.info('Getting operation %s' % operation_id)
		if operation_id in self.local_operations:
			return self.local_operations[operation_id]

		self.logger.warn("Operation with id %s not found" % operation_id)
		raise NotImplementedError("Operation with this id is not implemented")
