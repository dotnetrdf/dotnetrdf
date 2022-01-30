# Build generated files
require 'rdf/turtle'
require 'json/ld'
require 'haml'

task default: :index

MANIFESTS = Dir.glob("**/manifest*.ttl")

desc "Build HTML manifests"
task index: MANIFESTS.
  select {|m| File.exist?(m)}.
  map {|m| m.sub(/manifest(.*)\.ttl$/, 'index\1.html')}

MANIFESTS.each do |ttl|
  html = ttl.sub(/manifest(.*)\.ttl$/, 'index\1.html')

  # Find frame closest to file
  frame, template = nil, nil
  Pathname.new(ttl).ascend do |p|
    f = File.join(p, 'manifest-frame.jsonld')
    frame ||= f if File.exist?(f)

    t = File.join(p, 'template.haml')
    template ||= t if File.exist?(t)
  end
  frame ||= 'manifest-frame.jsonld'

  if template
    desc "Build #{html}"
    file html => [ttl, frame, template] do
      puts "Generate #{html}"
      temp, man = File.read(template), nil

      RDF::Reader.open(ttl) do |reader|
        out = JSON::LD::Writer.buffer(frame: frame, simple_compact_iris: true) do |writer|
          writer << reader
        end

        man = JSON.parse(out)
        if man['@graph'].is_a?(Array) && man['@graph'].length == 1
          # Remove '@graph'
          man['@graph'][0].each do |k, v|
            man[k] = v
          end
          man.delete('@graph')

          # Remove nil 'entries'
          man.delete('mf:entries') if man['mf:entries'].nil?

          # Fix up test entries
          Array(man['entries']).each do |entry|
            # Fix results which aren't IRIs
            if res = entry['mf:result'] && entry['mf:result']['@value']
              entry.delete('mf:result')
              entry['result'] = res == 'true'
            end

            # Fix some empty arrays (rdf-mt)
            %w(recognizedDatatypes unrecognizedDatatypes).each do |p|
              if entry["mf:#{p}"].is_a?(Hash) && entry["mf:#{p}"]['@list'] == []
                entry[p] = []
                entry.delete("mf:#{p}")
              end
            end
          end
        else
          Kernel.abort "Expected manifest to have a single @graph entry"
        end
      end

      File.open(html, "w") do |f|
        f.write Haml::Engine.new(temp, format: :html5).
          render(self,
            man: man,
            ttl: ttl.split('/').last
          )
      end
    end
  end
end
