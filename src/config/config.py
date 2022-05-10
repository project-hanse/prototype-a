def get_terminal_operation_ids():
    return ['067c7cd4-87f6-43e2-a733-26e5c51ef875', '0b60e908-fae2-4d33-aa81-5d1fdc706c12']


def get_api_base_url_pipeline_service():
    return "https://hanse.allteams.at/api/pipeline"


def get_api_base_url_learning_service():
    return "https://hanse.allteams.at/api/learning"


def get_api_user():
    return 'api_user'


def get_api_secret():
    return 'd49e3f0f-964e-4241-9693-31d2c80e5ecd'


load_open_ml_operation = {
    "operationId": "9c876745-9d61-4b0b-a32a-2de523b44d0b",
    "operationName": "load_data_from_openml",
    "operationFullName": "openml.datasets.load",
    "inputTypes": [],
    "outputType": None,
    "outputTypes": [
        2,
        1
    ],
    "framework": "openml",
    "frameworkVersion": "1.0.0",
    "sectionTitle": "OpenML",
    "title": "openml.datasets.load",
    "description": "Fetch dataset from OpenML by name or dataset id.",
    "signature": "load_data_from_openml",
    "signatureName": None,
    "returnsDescription": None,
    "sourceUrl": "https://scikit-learn.org/stable/modules/generated/sklearn.datasets.fetch_openml.html",
    "defaultConfig": {
        "name": None,
        "version": "active",
        "data_id": None,
        "data_home": None,
        "target_column": "default-target",
        "cache": "true"
    },
    "returns": "Dataframes"
}
