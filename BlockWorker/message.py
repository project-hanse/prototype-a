import json


class Message:
    def __init__(self, j):
        self.__dict__ = json.loads(j)

    def get_id(self):
        return self.Id

    def get_pipeline_id(self):
        return self.PipelineId

    def get_message(self):
        return self.Message
