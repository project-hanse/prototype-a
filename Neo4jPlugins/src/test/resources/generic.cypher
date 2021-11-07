CREATE (A:SomeNode {title: 'Node A', id: 'c32d6a33-eff6-49ec-b226-bf5f57dad7ea'})
CREATE (B:SomeNode {title: 'Node B', id: '57256819-cabf-4bdd-870b-f32f1798e407'})
CREATE (C:SomeNode {title: 'Node C', id: 'bc526bbb-d43b-4ced-a87d-4f16bcb415e8'})


CREATE
  (A)-[:HAS_SUCCESSOR]->(B),
  (B)-[:HAS_SUCCESSOR]->(C)
