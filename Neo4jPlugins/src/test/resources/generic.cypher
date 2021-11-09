CREATE (SA:SimpleNode {title: 'Node A', id: 'c32d6a33-eff6-49ec-b226-bf5f57dad7ea'})
CREATE (SB:SimpleNode {title: 'Node B', id: '57256819-cabf-4bdd-870b-f32f1798e407'})
CREATE (SC:SimpleNode {title: 'Node C', id: 'bc526bbb-d43b-4ced-a87d-4f16bcb415e8'})


CREATE
  (SA)-[:HAS_SUCCESSOR]->(SB),
  (SB)-[:HAS_SUCCESSOR]->(SC)


CREATE (A:Node {title: 'Node A', id: 'c568a9c6-bf59-410b-ad9f-0dffbe05d770'})
CREATE (B:Node {title: 'Node B', id: '0f4c454e-002d-47ca-84c1-d9ffd71addb5'})
CREATE (C:Node {title: 'Node C', id: '3aeb7e39-3ab8-42f1-b257-3521486eb6fb'})
CREATE (D:Node {title: 'Node D', id: 'c20c5d0a-b699-456b-ab23-e75d986d3576'})
CREATE (E:Node {title: 'Node E', id: 'c20c5d0a-b699-456b-ab23-e75d986d3576'})
CREATE (F:Node {title: 'Node F', id: 'a8e3f624-cdce-40b3-be2d-3acf138453f8'})
CREATE (G:Node {title: 'Node G', id: '3008d7cc-8e6c-4564-903f-d8f95ad0fdea'})

CREATE
  (A)-[:HAS_SUCCESSOR]->(B),
  (B)-[:HAS_SUCCESSOR]->(C),
  (C)-[:HAS_SUCCESSOR]->(G),
  (D)-[:HAS_SUCCESSOR]->(E),
  (E)-[:HAS_SUCCESSOR]->(B),
  (B)-[:HAS_SUCCESSOR]->(F),
  (E)-[:HAS_SUCCESSOR]->(F),
  (F)-[:HAS_SUCCESSOR]->(G)