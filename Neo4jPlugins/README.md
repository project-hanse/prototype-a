# Neo4j Procedure Plugins

**Based on https://github.com/neo4j-examples/neo4j-procedure-template.**

This project is an hanse you can use to build user defined procedures, functions and aggregation functions in Neo4j.
It contains two procedures, for reading and updating a full-text index.

To try this out, simply clone this repository and have a look at the source and test code (including Test-Server-Setup).

[Note]
This project requires a Neo4j {branch}.x dependency.

### User Defined Procedure

The user defined procedure allows you to get the incoming and outgoing relationships for a given node.

See link:{root}/main/java/hanse/GetRelationshipTypes.java[`GetRelationshipTypes.java`] and the link:
{root}/test/java/hanse/GetRelationshipTypesTests.java[`GetRelationshipTypesTests.java`].

```cypher
MATCH (n:Person)
CALL hanse.getRelationshipTypes(n);
```

### User Defined Function

The user defined function is a simple join function that joins a list of strings using a delimiter.

See link:{root}/main/java/hanse/Join.java[`Join.java`] and the link:
{root}/test/java/hanse/JoinTest.java[`JoinTest.java`].

```cypher
RETURN hanse.join(['A','quick','brown','fox'],' ') as sentence
```

=== User Defined Aggregation Function

The aggregation function `hanse.last` returns the last row of an aggregation.

```cypher
MATCH (n:Person)
WITH n ORDER BY n.born RETURN n.born, hanse.last(n) as last
```

See link:{root}/main/java/hanse/Last.java[`Last.java`] and the link:
{root}/test/java/hanse/LastTest.java[`LastTest.java`].

## Building

This project uses maven, to build a jar-file with the procedure in this project, simply package the project with maven:

    mvn clean package

This will produce a jar-file,`target/procedure-template-1.0.0-SNAPSHOT.jar`, that can be deployed in the `plugin`
directory of your Neo4j instance.

## License

Apache License V2, see LICENSE
