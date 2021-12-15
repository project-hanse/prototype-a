import logging
from typing import Callable

import pandas as pd

from src.services.operations_double_input_scikit_wrappers import OperationsDoubleInputSciKitWrappers
from src.services.operations_file_input import OperationsFileInputCollection
from src.services.operations_plots_matplotlib import PlotsMatPlotLib
from src.services.operations_single_input_pd_custom import OperationsSingleInputPandasCustom
from src.services.operations_single_input_pd_wrappers import OperationsSingleInputPandasWrappers


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
		self.local_operations[
			"2878ca36-8e03-4825-8afa-552064686337"] = OperationsDoubleInputSciKitWrappers.sklearn_double_input_predict

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

	def get_simple_operation_by_id(self, operation_id: str) -> Callable:
		self.logger.info('Getting simple operation %s' % operation_id)
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
