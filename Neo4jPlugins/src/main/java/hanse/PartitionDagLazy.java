package hanse;

import dto.PartitionResult;
import org.neo4j.graphdb.Direction;
import org.neo4j.graphdb.Node;
import org.neo4j.graphdb.Relationship;
import org.neo4j.graphdb.Transaction;
import org.neo4j.logging.Log;
import org.neo4j.procedure.*;

import java.util.List;
import java.util.UUID;
import java.util.stream.Stream;

public class PartitionDagLazy {

	@Context
	public Transaction tx;

	@Context
	public Log log;

	/**
	 * Traverses all nodes in the graph and partitions them into multiple groups.
	 * Implements Lazy DAG Partitioning algorithm.
	 * Starts at an array of nodes and finds all nodes that are connected to them via an incoming relationship with a given name.
	 * It then sets the _level property of all nodes depending on the dependency structure of a graph.
	 * <p>
	 * Example: Node A has a relationships 'HAS_SUCCESSOR' to Node B, and Node B has a relationship 'HAS_SUCCESSOR' to
	 * Node C. Given starting the traversal as Node C and the relationship parameter 'HAS_SUCCESSOR', Node C will be
	 * assigned level 0, Node B will be assigned level 1, and Node A will be assigned level 2.
	 *
	 * @param startNodes   the nodes to start the traversal from
	 * @param relationship the name of the relationship that should be followed
	 * @param depth        maximum depth of traversal
	 * @return the visited stamp (unique for this traversal) and the maximum level assigned to a node
	 */
	@Procedure(value = "hanse.partition.lazy", mode = Mode.WRITE)
	@Description("Assigns t-levels to all nodes by traversing all nodes that have an incoming relationship of a given name starting from a given set of nodes and sets labels in the nodes")
	public Stream<PartitionResult> partitionLazyStrategy(@Name("startNodes") List<Node> startNodes,
																											 @Name("relationship") String relationship,
																											 @Name("depth") long depth) {
		var result = new PartitionResult();

		result.visitedStamp = UUID.randomUUID().toString();
		log.info("Starting partitioning of graph resulting in lazy strategy (visitedStamp: {})...", result.visitedStamp);

		result.maxLevel = 0L;
		for (Node startNode : startNodes) {
			result.maxLevel = Math.max(visitNode(startNode, result.visitedStamp, 0L, relationship, depth), result.maxLevel);
		}

		log.debug("Partitioned graph with maximum level {} (visitedStamp: {})", result.maxLevel, result.visitedStamp);
		log.debug("Inverting levels (visitedStamp: {})...", result.visitedStamp);
		for (Node node : startNodes) {
			invertLevelValue(node, result.visitedStamp, result.maxLevel, relationship);
		}

		log.info("Finished partitioning of graph resulting in lazy strategy (visitedStamp: {}, maxLevel: {})", result.visitedStamp, result.maxLevel);
		return Stream.of(result);
	}

	private void invertLevelValue(Node node, String visitedStamp, long maxLevel, String relationship) {
		if (node.hasProperty("_inverted") && visitedStamp.equals(node.getProperty("_inverted"))) {
			return;
		}
		log.debug("Inverting level of node {} (visitedStamp: {})", node.getId(), visitedStamp);
		node.setProperty("_inverted", visitedStamp);
		node.setProperty("_level", maxLevel - (long) node.getProperty("_level"));
		for (Relationship r : node.getRelationships(Direction.INCOMING)) {
			if (r.getType().name().equals(relationship)) {
				invertLevelValue(r.getStartNode(), visitedStamp, maxLevel, relationship);
			}
		}
	}

	private long visitNode(Node node, String visitedStamp, long level, String relationship, long maxDepth) {
		log.debug("Visiting node {} with level {} for visitedStamp {}", node.getId(), level, visitedStamp);
		var subLevel = level;
		if (subLevel > maxDepth) {
			return maxDepth;
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
}

