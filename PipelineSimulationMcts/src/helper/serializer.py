import uuid
from ipaddress import IPv4Address
from json import JSONEncoder

from openml.tasks import TaskType


class TMCSerializer(JSONEncoder):

    def default(self, value: any) -> str:
        """
        JSON serialization conversion function based on https://stackoverflow.com/a/64395300/11016410.
        """

        # If it's an IP, which is not normally
        # serializable, convert to string.
        if isinstance(value, IPv4Address):
            return str(value)

        # Here you can have other handling for your
        # UUIDs, or datetimes, or whatever else you
        # have.
        if isinstance(value, uuid.UUID):
            return str(value)

        if isinstance(value, TaskType):
            return str(value)

        # Otherwise, default to super
        return super(TMCSerializer, self).default(value)
