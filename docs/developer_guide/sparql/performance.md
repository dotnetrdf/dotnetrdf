# SPARQL Performance

This page provides information on the performance of our SPARQL engine.

## Performance Limitations 

### Memory Usage

Our in-memory [Leviathan engine](leviathan_engine.md) is designed to scale to a few million triples in-memory.  However, scalability will vary heavily with your data and queries e.g. data with lots of large literals will require more memory and queries that generate large intermediate results will also require lots of memory.

Memory usage may be affected by how you load your data, see [How To: Minimize Memory Usage](../../user_guide/howto/minimize_memory_usage.md) for some discussion of configuring indexes to reduce memory usage.

We recommend that if you have more than a million triples you switch to using one of the many supported third party triple stores, see [Storage Providers](../../user_guide/configuration/storage_providers.md) for the available implementations.

### Multi-Core Systems

dotNetRDF leverages the .Net PLINQ feature to parallelize some aspects of query evaluation that may significantly improve performance over running in single core mode.  However not all queries can be safely parallelized, so you may see varying results.
