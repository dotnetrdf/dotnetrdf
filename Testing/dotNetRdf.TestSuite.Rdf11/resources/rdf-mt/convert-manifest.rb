#!/usr/bin/env ruby
# Convert 2004 manifest to 2013 format and vocabulary
require 'linkeddata'
require 'fileutils'

TEST = RDF::Vocabulary.new("http://www.w3.org/2000/10/rdf-tests/rdfcore/testSchema#")
QUERY = SPARQL.parse(%(
  PREFIX test: <http://www.w3.org/2000/10/rdf-tests/rdfcore/testSchema#>
  SELECT ?subject ?type ?description ?premise ?conclusion
  WHERE {
    ?subject a ?type;
      test:status "APPROVED";
      test:premiseDocument ?premise;
      test:conclusionDocument ?conclusion;
      OPTIONAL {
        ?subject test:description ?description
      }
    FILTER(?type = test:PositiveEntailmentTest || ?type = test:NegativeEntailmentTest)
  }
))

g = RDF::Repository.load("2004-test-suite/Manifest.rdf")

tests = {}

File.open("manifest.ttl", "w") do |f|
  f.write(%(
    # RDF Schema and Semantics tests
    ## Distributed under both the W3C Test Suite License[1] and the W3C 3-
    ## clause BSD License[2]. To contribute to a W3C Test Suite, see the
    ## policies and contribution forms [3]
    ##
    ## 1. http://www.w3.org/Consortium/Legal/2008/04-testsuite-license
    ## 2. http://www.w3.org/Consortium/Legal/2008/03-bsd-license
    ## 3. http://www.w3.org/2004/10/27-testcases
    
    @prefix rdf:    <http://www.w3.org/1999/02/22-rdf-syntax-ns#> .
    @prefix rdfs:    <http://www.w3.org/2000/01/rdf-schema#> .
    @prefix mf: <http://www.w3.org/2001/sw/DataAccess/tests/test-manifest#> .
    @prefix rdft:   <http://www.w3.org/ns/rdftest#> .

    <> rdf:type mf:Manifest ;
      rdfs:comment "RDF Schema and Semantics tests" ;
      mf:entries \()[1..-1].gsub(/^    /, ''))

  QUERY.execute(g).each do |soln|
    #puts soln.inspect
    dir = soln.subject.path.split('/')[-2]
    frag = "##{dir}-#{soln.subject.fragment}"
    puts "test #{dir}/'#{frag}' already defined" if tests.has_key?(frag)
    f.write("\n    <#{frag}>")
    tests[frag] = soln
  end
  f.puts("\n  ) .\n\n")

  tests.each_pair do |frag, test|
    # Wrap description to 40 characters and indent
    desc = test.description.
      to_s.
      strip.
      gsub(/\s+/m, ' ').
      scan(/\S.{0,60}\S(?=\s|$)|\S+/).
      join("\n    ")

    [:premise, :conclusion].each do |t|
      test[t] = if test[t].node?
        'false'
      else
        path = test[t].path.split('/')[-2..-1].join('/')

        # Copy the test into place, if it does not exist.
        # If it is an RDF/XML file, transform it to Turtle
        parts = path.split('/')

        FileUtils.mkdir(parts.first) unless Dir.exist?(parts.first)
        if File.exist?(path.sub('.rdf', '.ttl'))
          path.sub!('.rdf', '.ttl')
        else
          if parts.last =~ /\.rdf/
            puts "Transform 2004-test-suite/#{path} to #{path.sub('.rdf', '.ttl')}"
            RDF::RDFXML::Reader.open("2004-test-suite/#{path}") do |reader|
              doc = reader.instance_variable_get(:@doc)

              # Retain comment
              comment = doc.children.detect {|c| c.is_a?(Nokogiri::XML::Comment)}
              comment = comment ? comment.content : ""
              test_graph = RDF::Graph.new << reader
              ttl = test_graph.dump(:ttl, :standard_prefixes => true, :prefixes => {
                :test => TEST.to_uri
              })
              parts[1] = parts.last.sub('.rdf', '.ttl')
              path = parts.join("/")
              File.open(path, "w") do |ttl_file|
                # Output existing comment
                comment.lines.each do |line|
                  ttl_file.write("# #{line}")
                end
                ttl_file.write(ttl)
              end
            end
          else
            puts "Copy 2004-test-suite/#{path} to #{path}"
            FileUtils.cp "2004-test-suite/#{path}", path
          end
        end

        # Use this relative path in manifest
        "<#{path}>"
      end
    end

    f.puts(%(
      <#{frag}> a mf:#{test.type.fragment};
        mf:name "#{frag[1..-1]}";
        rdfs:comment """
          #{desc}
        """;
        rdft:approval rdft:Approved;
        mf:action #{test.premise};
        mf:result #{test.conclusion} .
    )[1..-1].gsub(/^      /, ''))
  end
end

