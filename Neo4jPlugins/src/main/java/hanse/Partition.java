package hanse;

import org.neo4j.graphdb.Direction;
import org.neo4j.graphdb.Node;
import org.neo4j.graphdb.Relationship;
import org.neo4j.graphdb.Transaction;
import org.neo4j.logging.Log;
import org.neo4j.procedure.Context;
import org.neo4j.procedure.Mode;
import org.neo4j.procedure.Name;
import org.neo4j.procedure.Procedure;

import java.util.List;
import java.util.UUID;
import java.util.stream.Stream;

public class Partition {

    @Context
    public Transaction tx;

    @Context
    public Log log;

    /**
     * Traverses all nodes in the graph and partitions them into multiple groups.
     * Starts at an array of nodes and finds all nodes that are connected to them via an incoming relationship with a given name.
     * It then sets the _level property of all nodes depending on the dependency structure of a graph.
     * <p>
     * Example: Node A has a relationships 'HAS_SUCCESSOR' to Node B, and Node B has a relationship 'HAS_SUCCESSOR' to
     * Node C. Given a condition that matches Node C and the relationship 'HAS_SUCCESSOR', Node A and Node B will be
     * returned.
     *
     * @param startNodes   the nodes to start the traversal from
     * @param relationship the name of the relationship that should be followed
     * @param depth        maximum depth of traversal
     * @return the visited stamp and the maximum level assigned to a node
     */
    @Procedure(value = "hanse.markPartitions", mode = Mode.WRITE)
    public Stream<NodeGroups> markPartitions(@Name("startNodes") List<Node> startNodes,
                                             @Name("relationship") String relationship,
                                             @Name("depth") long depth) {
        var groups = new NodeGroups();

        groups.visitedStamp = UUID.randomUUID().toString();
        groups.maxLevel = 0L;
        for (Node startNode : startNodes) {
            groups.maxLevel = Math.max(visitNode(startNode, groups.visitedStamp, groups.maxLevel, relationship, depth), groups.maxLevel);
        }

        return Stream.of(groups);
    }

    private long visitNode(Node node, String visitedStamp, long level, String relationship, long maxDepth) {
        var subLevel = level;
        if (subLevel > maxDepth) {
            return subLevel;
        }
        if (node.hasProperty("_visited") && visitedStamp.equals(node.getProperty("_visited"))) {
            var existingLevel = (long) node.getProperty("_level");
            if (existingLevel < level) {
                node.setProperty("_level", level);
            }
        } else {
            node.setProperty("_level", level);
            node.setProperty("_visited", visitedStamp);
        }
        if (!node.hasRelationship(Direction.INCOMING)) {
            return subLevel;
        }
        for (Relationship r : node.getRelationships(Direction.INCOMING)) {
            if (r.getType().name().equals(relationship)) {
                subLevel = Math.max(visitNode(r.getStartNode(), visitedStamp, level + 1, relationship, maxDepth), subLevel);
            }
        }
        return subLevel;
    }

    public static final class NodeGroups {
        public long maxLevel;
        public String visitedStamp;
    }

}

