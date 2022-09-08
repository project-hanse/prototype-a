package hanse;

import org.junit.jupiter.api.BeforeAll;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.TestInstance;
import org.neo4j.driver.Config;
import org.neo4j.driver.GraphDatabase;
import org.neo4j.harness.Neo4j;
import org.neo4j.harness.Neo4jBuilders;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;
import java.io.StringWriter;
import java.util.Objects;

import static org.assertj.core.api.Assertions.assertThat;
import static org.neo4j.driver.Values.parameters;

@TestInstance(TestInstance.Lifecycle.PER_CLASS)
class PartitionDagEagerTest {

	private static final Config driverConfig = Config.builder().withoutEncryption().build();
	private Neo4j embeddedDatabaseServer;

	@BeforeAll
	void initializeNeo4j() throws IOException {

		var sw = new StringWriter();
		try (var in = new BufferedReader(new InputStreamReader(Objects.requireNonNull(getClass().getResourceAsStream("/generic.cypher"))))) {
			in.transferTo(sw);
			sw.flush();
		}

		this.embeddedDatabaseServer = Neo4jBuilders.newInProcessBuilder()
			.withProcedure(PartitionDagEager.class)
			.withFixture(sw.toString())
			.build();
	}

	@Test
	void partitionSimple() {

		try (var driver = GraphDatabase.driver(embeddedDatabaseServer.boltURI(), driverConfig);
				 var session = driver.session()) {

			var result = session.run("MATCH (n:SimpleNode {id: 'c32d6a33-eff6-49ec-b226-bf5f57dad7ea'}) WITH collect(n) AS nodesList CALL hanse.partition.eager(nodesList, 'HAS_SUCCESSOR', 3) YIELD maxLevel, visitedStamp RETURN maxLevel, visitedStamp").single();

			assertThat(result).isNotNull();
			assertThat(result.get("maxLevel").asLong()).isEqualTo(2);
			assertThat(result.get("visitedStamp").asString()).isNotNull();
		}
	}

	@Test
	void partitionSimpleShouldStopAtMaxDepth() {

		try (var driver = GraphDatabase.driver(embeddedDatabaseServer.boltURI(), driverConfig);
				 var session = driver.session()) {

			var result = session.run("MATCH (n:SimpleNode {id: 'c32d6a33-eff6-49ec-b226-bf5f57dad7ea'}) WITH collect(n) AS nodesList CALL hanse.partition.eager(nodesList, 'HAS_SUCCESSOR', 1) YIELD maxLevel, visitedStamp RETURN maxLevel, visitedStamp").single();
			var visitedStamp = result.get("visitedStamp").asString();

			assertThat(result).isNotNull();
			assertThat(result.get("maxLevel").asLong()).isEqualTo(1);
			assertThat(result.get("visitedStamp").asString()).isNotNull();
			assertThat(session.run("MATCH (n:SimpleNode {_visited: $stamp_id}) RETURN n", parameters("stamp_id", visitedStamp)).list().size()).isEqualTo(2);
		}
	}

	@Test
	void partitionShouldTraverseEntireGraph() {

		try (var driver = GraphDatabase.driver(embeddedDatabaseServer.boltURI(), driverConfig);
				 var session = driver.session()) {

			var result = session.run("MATCH (n:Node) WHERE n.id = 'c568a9c6-bf59-410b-ad9f-0dffbe05d770' OR n.id = 'c20c5d0a-b699-456b-ab23-e75d986d3576' WITH collect(n) AS nodesList CALL hanse.partition.eager(nodesList, 'HAS_SUCCESSOR', 10) YIELD maxLevel, visitedStamp RETURN maxLevel, visitedStamp").single();
			var visitedStamp = result.get("visitedStamp").asString();

			assertThat(result).isNotNull();
			assertThat(result.get("maxLevel").asLong()).isEqualTo(4);
			assertThat(result.get("visitedStamp").asString()).isNotNull();
			assertThat(session.run("MATCH (n:Node {_visited: $stamp_id}) RETURN n", parameters("stamp_id", visitedStamp)).list().size()).isEqualTo(7);
			assertThat(session.run("MATCH (n:Node {_visited: $stamp_id, _level: 0}) RETURN n", parameters("stamp_id", visitedStamp)).list().size()).isEqualTo(2);
			assertThat(session.run("MATCH (n:Node {_visited: $stamp_id, _level: 1}) RETURN n", parameters("stamp_id", visitedStamp)).list().size()).isEqualTo(1);
			assertThat(session.run("MATCH (n:Node {_visited: $stamp_id, _level: 2}) RETURN n", parameters("stamp_id", visitedStamp)).list().size()).isEqualTo(1);
			assertThat(session.run("MATCH (n:Node {_visited: $stamp_id, _level: 3}) RETURN n", parameters("stamp_id", visitedStamp)).list().size()).isEqualTo(2);
			assertThat(session.run("MATCH (n:Node {_visited: $stamp_id, _level: 4}) RETURN n", parameters("stamp_id", visitedStamp)).list().size()).isEqualTo(1);
		}
	}

	// TODO add tests for cyclic graphs
}
