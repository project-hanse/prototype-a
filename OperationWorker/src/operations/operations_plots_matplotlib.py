import logging

import numpy as np

from src.helper.operations_helper import OperationsHelper


class PlotsMatPlotLib:

	@staticmethod
	def matplot_plot_pd(logger: logging, operation_name: str, operation_config: dict, data: []) -> []:
		logger.info("Plotting dataframe with matplotlib backend '%s'", operation_name)

		OperationsHelper.validate_input_or_throw(data, 1)
		df = data[0]

		output_file_path = OperationsHelper.get_or_throw(operation_config, 'output_file_path')
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

		if type(plots) == np.ndarray:
			if len(plots) > 1:
				logger.info('Produced multiple plots using first one')
				plots = [plots[0]]
			fig = plots[0].get_figure()
			fig.savefig(output_file_path)
		else:
			fig = plots.get_figure()
			fig.savefig(output_file_path)

		logger.info('Saved plot to temporary file %s' % output_file_path)
