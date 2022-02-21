#!/usr/bin/env ruby
require "bundler/setup"
require 'json/ld'
require 'rdf/turtle'
require 'rdf/isomorphic'
require 'rdf/ordered_repo'
require 'rdf/normalize'

MAN_DIR = File.expand_path("")
LOCAL_CTX = JSON.parse(File.read("#{MAN_DIR}/manifest-context.jsonld"))

%w{
  nt/syntax/manifest.ttl
  semantics/manifest.ttl
  sparql/eval/manifest.ttl
  sparql/syntax/manifest.ttl
  trig/eval/manifest.ttl
  trig/syntax/manifest.ttl
  turtle/eval/manifest.ttl
  turtle/syntax/manifest.ttl
}.each do |src|
  dst = src.sub('.ttl', '.jsonld')
  file_src = File.expand_path(src)
  file_dst = File.expand_path(dst)

  base = "https://w3c.github.io/rdf-star/tests/#{src}/".sub(/manifest.*$/, '')
  trs = base[0..-2] + '#'

  ttl_graph = RDF::OrderedRepo.load(file_src, base_uri: base)
  local_ctx = LOCAL_CTX.dup
  local_ctx['@context']['@base'] = base
  local_ctx['@context']['trs'] = trs
  JSON::LD::Writer.open(file_dst,
    format: :jsonld, 
    frame: local_ctx,
    ordered: true,
    context: local_ctx,
    useNativeTypes: true,
    base_uri: base) {|writer| writer << ttl_graph}

  # Validate that the two graphs say the same thing.
  jsonld_graph = RDF::OrderedRepo.load(file_dst, base_uri: base)
  if !jsonld_graph.isomorphic?(ttl_graph)
    STDERR.puts "expected #{file_dst} to be isomorphic with expected #{file_src}"
    STDERR.puts "\nFrom Turtle:\n#{ttl_graph.dump(:normalize)}"
    STDERR.puts "\nFrom JSON-LD:\n#{jsonld_graph.dump(:normalize)}"
    exit(1)
  end
end

