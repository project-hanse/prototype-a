# An extension script executed before neo4j db starts.
# See https://neo4j.com/docs/operations-manual/4.4/docker/configuration/

echo "Moving apoc .jar to plugins directory"
mv "$NEO4J_HOME"/labs/apoc-*.jar "$NEO4J_HOME"/plugins/

echo "Enabling file import and export"

# Append lines to neo4j.conf
echo "#********************************************************************" >>"$NEO4J_HOME"/conf/neo4j.conf
echo "# Custom Neo4j configurations" >>"$NEO4J_HOME"/conf/neo4j.conf
echo "#********************************************************************" >>"$NEO4J_HOME"/conf/neo4j.conf
echo "" >>"$NEO4J_HOME"/conf/neo4j.conf
echo "apoc.export.file.enabled=true" >>"$NEO4J_HOME"/conf/neo4j.conf
echo "apoc.import.file.enabled=true" >>"$NEO4J_HOME"/conf/neo4j.conf
