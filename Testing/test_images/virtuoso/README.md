# Virtuoso

Virtuoso 7 (stable) Open Source Edition on Ubuntu 14.04

Note that this is based on the current `stable/7` branch of Virtuoso. 

## Docker image

[stain/virtuoso](https://registry.hub.docker.com/u/stain/virtuoso/)


## License

* Dockerfile and scripts: [Apache Software License 2.0](http://www.apache.org/licenses/LICENSE-2.0)
* Docker image: [GNU GPL 2](https://github.com/openlink/virtuoso-opensource/blob/stable/7/LICENSE), like Virtuoso.


## Credits

[Virtuoso Open Source Edition](https://github.com/openlink/virtuoso-opensource)
(C) 1998-2015 [OpenLink Software](http://www.openlinksw.com/)
<vos.admin@openlinksw.com>

Docker image maintained by [Stian Soiland-Reyes](http://orcid.org/0000-0001-9842-9718) on behalf of the 
[Open PHACTS Foundation](http://www.openphactsfoundation.org/), based on
[ansible-role-virtuoso](https://github.com/nicholsn/ansible-role-virtuoso) by
[Nolan Nichols](http://orcid.org/0000-0003-1099-3328) 


## Usage

This docker image exposes ports `8890` (SPARQL/WebDAV) and `1111` (isql).

Virtuoso data is stored in the volume `/virtuoso`. If needed, you can modify
the `virtuoso.ini` by using `/etc/virtuoso-opensource-7` as a volume.

Example of running Virtuoso to directly expose port `8890`:

    docker run -p 8890:8890 -d stain/virtuoso

You may want to specify an explicit volume path on the host to use a
particular host partition (e.g. for storage or speed requirements).
For example, to make Virtuoso use the folder `/ssd/virtuoso` on the host:

    docker run -v /ssd/virtuoso/:/virtuoso -p 8890:8890 -d stain/virtuoso

Note that only a single container can access the `/virtuoso` volume at a time, otherwise you'll get:

	14:36:57 Unable to lock file /virtuoso/virtuoso.lck (Resource temporarily unavailable).
	14:36:57 Virtuoso is already runnning (pid 0)

## Data volumes

It is good practice to use [data volumes](http://docs.docker.com/userguide/dockervolumes/) to
keep Virtuoso's data persistent across upgrades of the Virtuoso Docker image. 

For example, to create a data container called `virtuoso-data` for the volume `/virtuoso`, and link to it from
a Virtuoso container called `virtuoso`:

    docker run --name virtuoso-data -v /virtuoso busybox
    docker run --name virtuoso --volumes-from virtuoso-data -p 8890:8890 -d stain/virtuoso

By using a named Docker container with `--name`, you can more easily stop and restart Virtuoso, e.g:

    docker stop virtuoso
    docker start virtuoso

To make Virtuoso start on boot and restart on failure, create the container with `--restart=always`:

    docker run --name virtuoso --restart=always --volumes-from virtuoso-data -p 8890:8890 -d stain/virtuoso

If you need to replace an already created named container, remove the old container(s) first, including `-v` to  also delete the volume content:
  
    docker rm -v virtuoso
    docker rm -v virtuoso-data # WARNING: Removes all data!



## Staging

The command `staging.sh` can be used with the volume `/staging` and the file
`/staging/staging.sql` to load data, typically using 
[ld\_dir](http://virtuoso.openlinksw.com/dataspace/doc/dav/wiki/Main/VirtBulkRDFLoader)

First, if you are populating to an existing data volume, ensure that the corresponding
Virtuoso instance is **not running**:

    docker stop virtuoso

To populate the *data volume* `virtuoso-data` (see above) by running
`staging.sql` from `/data/rdf` on the host:

    docker run -v /data/rdf:/staging:ro --volumes-from virtuoso-data -it stain/virtuoso staging.sh

(This uses the `:ro` parameter as Virtuoso would not be writing to its `/staging`.)

Note that the equivalent of `/data/rdf/staging.sql` must use `/staging` as base directory, example:

    -- Gene Ontology
    ld_dir('/staging/GO' , 'go_daily-termdb.owl.gz' , 'http://www.geneontology.org' );
    ld_dir('/staging/GO' , 'goTreeInference.ttl.gz', 'http://www.geneontology.org/inference');
    ld_dir('/staging/GO' , 'go_daily-termdb.nt.gz' , 'http://www.geneontology.org/terms' );

    -- GOA
    ld_dir('/staging/GOA' , '*.rdf.gz' , 'http://www.openphacts.org/goa' );

After staging is complete, a total number of triples (including any present
before staging) will be output.

For example:

    stain@docker:~$ docker stop virtuoso
    virtuoso
    
    stain@docker:~$ docker run -v /data/rdf:/staging:ro --volumes-from virtuoso-data -it stain/virtuoso staging.sh
    Populating from /staging/staging.sql
    Total number of triples:
    5752

    stain@docker:~$ docker restart virtuoso


## Connecting with isql

To run an interactive [isql](http://docs.openlinksw.com/virtuoso/isql.html) session
to connect to the docker container `myvirtuoso`, try:

    docker run --link myvirtuoso:virtuoso -it stain/virtuoso isql virtuoso

## Contribute

For this Docker image, feel free to
[contribute](https://github.com/stain/virtuoso-docker), e.g.  
[raise an issue](https://github.com/stain/virtuoso-docker/issues) or
a [pull request](https://github.com/stain/virtuoso-docker/pulls).

For all other questions on using and configuring Virtuoso, see the [Virtuoso
support forums](http://boards.openlinksw.com/support/)
and [virtuoso-opensource issues](https://github.com/openlink/virtuoso-opensource/issues).

