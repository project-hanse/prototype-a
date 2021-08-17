import threading

from src.services.in_memory_store import InMemoryStore


class ImportService:
    def __init__(self, in_memory_store: InMemoryStore) -> None:
        super().__init__()
        self.in_memory_store = in_memory_store

    def import_defaults_in_background(self):
        """
        Imports a number of datasets in a dedicated thread.
        """
        import_thread = threading.Thread(target=self.import_defaults, name="Importer")
        import_thread.start()

    def import_defaults(self):
        """
        Importing default datasets to in memory store.
        """
        self.in_memory_store.import_with_id("./datasets/MELBOURNE_HOUSE_PRICES_LESS.csv",
                                            "0c2acbdb-544b-4efc-ae54-c2dcba988654")
        self.in_memory_store.import_with_id("./datasets/influenca_vienna_2009-2018.csv",
                                            "4cfd0698-004a-404e-8605-de2f830190f2")
        self.in_memory_store.import_with_id("./datasets/weather_vienna_2009-2018.csv",
                                            "244b5f61-1823-48fb-b7fa-47a2699bb580")
        self.in_memory_store.import_with_id("./datasets/21211-003Z_format.csv",
                                            "2b88720f-8d2d-46c8-84d2-ab177c88cb5f")
        self.in_memory_store.import_with_id("./datasets/21311-001Z_format.csv",
                                            "61501213-d945-49a5-9212-506d6305af13")
        self.in_memory_store.import_with_id("./datasets/simulated-vine-yield-styria.xlsx",
                                            "1a953cb2-4ad1-4c07-9a80-bd2c6a68623a")

        # Load ZAMG data Graz Flughafen
        # for year in range(1990, 2021):
        #     base_uuid = "8d15d14d-2eba-4d36-b2ba-aaaaaaaa"
        #     self.in_memory_store.import_with_id(
        #         "./datasets/ZAMG/Steiermark/Graz_Flughafen/ZAMG_Jahrbuch_%s.csv" % year,
        #         "%s%s" % (base_uuid, year))
