import pandas as pd


def load_file(path: str) -> pd.DataFrame:
    if path.endswith(".csv"):
        df = pd.read_csv(path, sep=';')
    elif path.endswith(".xlsx"):
        df = pd.read_excel(path)
    else:
        raise NotImplementedError("Unsupported file type")

    return df
