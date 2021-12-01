import pandas as pd


def load_file(path: str, csv_sep: str = ';', skip_rows: int = None) -> pd.DataFrame:
    if path.endswith(".csv"):
        df = pd.read_csv(path, sep=csv_sep, skiprows=skip_rows)
    elif path.endswith(".xlsx"):
        df = pd.read_excel(path)
    else:
        raise NotImplementedError("Unsupported file type")

    return df
