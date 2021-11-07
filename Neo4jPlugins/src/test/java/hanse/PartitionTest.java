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

@TestInstance(TestInstance.Lifecycle.PER_CLASS)
class PartitionTest {

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
                .withProcedure(Partition.class)
                .withFixture(sw.toString())
                .build();
    }

    @Test
    void partitionSimple() {

        try (var driver = GraphDatabase.driver(embeddedDatabaseServer.boltURI(), driverConfig);
             var session = driver.session()) {

            var result = session.run("MATCH (n:SomeNode {id: 'bc526bbb-d43b-4ced-a87d-4f16bcb415e8'}) WITH collect(n) AS nodesList CALL hanse.markPartitions(nodesList, 'HAS_SUCCESSOR', 3) YIELD maxLevel, visitedStamp RETURN maxLevel, visitedStamp")
                    .single();

            assertThat(result).isNotNull();
            assertThat(result.get("maxLevel").asLong()).isEqualTo(2);
            assertThat(result.get("visitedStamp").asString()).isNotNull();
        }
    }
}
