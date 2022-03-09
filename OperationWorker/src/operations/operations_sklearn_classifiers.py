import logging

import pandas as pd
from sklearn.ensemble import RandomForestClassifier
from sklearn.svm import SVC

from src.helper.operations_helper import OperationsHelper


class OperationsSklearnClassifiers:

	@staticmethod
	def sklearn_create_fit_classifier_svc(logger: logging, operation_name: str, operation_config: dict, data: []):
		"""
		Create and fit a C-Support Vector classifier.
		"""
		logger.info("Executing scikit operation sklearn_create_fit_classifier_svc (%s)" % operation_name)
		OperationsHelper.validate_input_or_throw(data, 2)

		c = OperationsHelper.get_or_default(operation_config, "C", 1.0)
		kernel = OperationsHelper.get_or_default(operation_config, "kernel", "rbf")
		degree = OperationsHelper.get_or_default(operation_config, "degree", 3)
		gamma = OperationsHelper.get_or_default(operation_config, "gamma", "scale")
		coef0 = OperationsHelper.get_or_default(operation_config, "coef0", 0.0)
		shrinking = OperationsHelper.get_or_default(operation_config, "shrinking", True)
		probability = OperationsHelper.get_or_default(operation_config, "probability", False)
		tol = OperationsHelper.get_or_default(operation_config, "tol", 0.001)
		cache_size = OperationsHelper.get_or_default(operation_config, "cache_size", 200)
		class_weight = OperationsHelper.get_or_default(operation_config, "class_weight", None)
		verbose = OperationsHelper.get_or_default(operation_config, "verbose", False)
		max_iter = OperationsHelper.get_or_default(operation_config, "max_iter", -1)
		decision_function_shape = OperationsHelper.get_or_default(operation_config, "decision_function_shape", "ovr")
		break_ties = OperationsHelper.get_or_default(operation_config, "break_ties", False)
		random_state = OperationsHelper.get_or_default(operation_config, "random_state", None)

		cls = SVC(C=c, kernel=kernel, degree=degree, gamma=gamma, coef0=coef0, shrinking=shrinking, probability=probability,
							tol=tol, cache_size=cache_size, class_weight=class_weight, verbose=verbose, max_iter=max_iter,
							decision_function_shape=decision_function_shape, break_ties=break_ties, random_state=random_state)

		cls.fit(data[0], data[1])

		return cls

	@staticmethod
	def sklearn_create_fit_classifier_random_forest(logger: logging, operation_name: str, operation_config: dict,
																									data: []):
		"""
		Create and fit a Random Forest classifier.
		"""
		logger.info("Executing scikit operation sklearn_create_fit_classifier_random_forest (%s)" % operation_name)
		OperationsHelper.validate_input_or_throw(data, 2)

		n_estimators = OperationsHelper.get_or_default(operation_config, "n_estimators", 100)
		criterion = OperationsHelper.get_or_default(operation_config, "criterion", "gini")
		max_depth = OperationsHelper.get_or_default(operation_config, "max_depth", None)
		min_samples_split = OperationsHelper.get_or_default(operation_config, "min_samples_split", 2)
		min_samples_leaf = OperationsHelper.get_or_default(operation_config, "min_samples_leaf", 1)
		min_weight_fraction_leaf = OperationsHelper.get_or_default(operation_config, "min_weight_fraction_leaf", 0.0)
		max_features = OperationsHelper.get_or_default(operation_config, "max_features", "auto")
		max_leaf_nodes = OperationsHelper.get_or_default(operation_config, "max_leaf_nodes", None)
		min_impurity_decrease = OperationsHelper.get_or_default(operation_config, "min_impurity_decrease", 0.0)
		bootstrap = OperationsHelper.get_or_default(operation_config, "bootstrap", True)
		oob_score = OperationsHelper.get_or_default(operation_config, "oob_score", False)
		n_jobs = OperationsHelper.get_or_default(operation_config, "n_jobs", None)
		random_state = OperationsHelper.get_or_default(operation_config, "random_state", None)
		verbose = OperationsHelper.get_or_default(operation_config, "verbose", 0)
		warm_start = OperationsHelper.get_or_default(operation_config, "warm_start", False)
		class_weight = OperationsHelper.get_or_default(operation_config, "class_weight", None)
		ccp_alpha = OperationsHelper.get_or_default(operation_config, "ccp_alpha", 0.0)
		max_samples = OperationsHelper.get_or_default(operation_config, "max_samples", None)

		cls = RandomForestClassifier(n_estimators=n_estimators, criterion=criterion, max_depth=max_depth,
																 min_samples_split=min_samples_split, min_samples_leaf=min_samples_leaf,
																 min_weight_fraction_leaf=min_weight_fraction_leaf, max_features=max_features,
																 max_leaf_nodes=max_leaf_nodes, min_impurity_decrease=min_impurity_decrease,
																 bootstrap=bootstrap, oob_score=oob_score, n_jobs=n_jobs, random_state=random_state,
																 verbose=verbose, warm_start=warm_start, class_weight=class_weight, ccp_alpha=ccp_alpha,
																 max_samples=max_samples)

		cls.fit(data[0], data[1])

		return cls

	@staticmethod
	def sklearn_classifier_score(logger: logging, operation_name: str, operation_config: dict, data: []):
		"""
		Scores a classifier.
		"""
		logger.info("Executing scikit operation sklearn_classifier_score (%s)" % operation_name)
		OperationsHelper.validate_input_or_throw(data, 3)

		cls = data[0]
		score = cls.score(data[1], data[2])
		metrics = {
			"score": score
		}

		df = pd.DataFrame(metrics, index=[0])

		return df

	@staticmethod
	def sklearn_classifier_predict(logger: logging, operation_name: str, operation_config: dict, data: []):
		"""
		Predicts labels using a classifier.
		"""
		logger.info("Executing scikit operation sklearn_classifier_predict (%s)" % operation_name)
		OperationsHelper.validate_input_or_throw(data, 2)

		prediction_column_name = OperationsHelper.get_or_default(operation_config, "prediction_column_name", 'prediction')

		cls = data[0]
		df = data[1]

		prediction = cls.predict(df)
		df[prediction_column_name] = prediction

		return df
