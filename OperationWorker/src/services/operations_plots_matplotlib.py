import logging
import os.path

import numpy as np
import pandas as pd

from src.helper.operations_helper import OperationsHelper
from src.models.dataset import Dataset


class PlotsMatPlotLib:

	@staticmethod
	def matplot_plot_pd(logger: logging, operation_config: dict, df: pd.DataFrame, dataset: Dataset) -> None:
		logger.info("Plotting dataframe with matplotlib backend")

		x = OperationsHelper.get_or_default(operation_config, 'x', None)
		y = OperationsHelper.get_or_default(operation_config, 'y', None)
		kind = OperationsHelper.get_or_default(operation_config, 'kind', 'line')
		ax = OperationsHelper.get_or_default(operation_config, 'ax', None)
		subplots = OperationsHelper.get_or_default(operation_config, 'subplots', False)
		sharex = OperationsHelper.get_or_default(operation_config, 'sharex', False)
		sharey = OperationsHelper.get_or_default(operation_config, 'sharey', False)
		layout = OperationsHelper.get_or_default(operation_config, 'layout', None)
		use_index = OperationsHelper.get_or_default(operation_config, 'use_index', True)
		title = OperationsHelper.get_or_default(operation_config, 'title', None)
		grid = OperationsHelper.get_or_default(operation_config, 'grid', None)
		legend = OperationsHelper.get_or_default(operation_config, 'legend', True)
		# TODO: add more options https://pandas.pydata.org/pandas-docs/stable/reference/api/pandas.DataFrame.plot.html

		try:
			plots = df.plot(x=x, y=y, kind=kind, ax=ax, subplots=subplots, sharex=sharex, sharey=sharey, layout=layout,
											use_index=use_index, title=title, grid=grid, legend=legend, backend='matplotlib', figsize=(16, 9))

		except Exception as e:
			logger.error("Error while plotting dataframe with matplotlib backend: %s" % str(e))
			raise e
		target_path = OperationsHelper.get_temporary_file_path(dataset)
		filename, file_extension = os.path.splitext(target_path)
		if file_extension == '':
			logger.warning("No file extension found for dataset '%s' store '%s'" % (dataset.get_key(), dataset.get_store()))
			raise ValueError("No file extension found for dataset '%s' store '%s'" % (dataset.get_key(), dataset.get_store()))

		if type(plots) == np.ndarray:
			if len(plots) > 1:
				logger.info('Produced multiple plots using first one')
				plots = [plots[0]]
			fig = plots[0].get_figure()
			fig.savefig(target_path)
		else:
			fig = plots.get_figure()
			fig.savefig(target_path)

		logger.info('Saved plot to temporary file %s' % target_path)
