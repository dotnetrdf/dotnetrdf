#!/usr/bin/env python
from pathlib import Path
import json
import os
import sys
from urllib.parse import urlparse

DIR = Path(__file__).parent

def main():
    for i in [
        ["semantics", "manifest.jsonld"],
        ["trig", "syntax", "manifest.jsonld"],
        ["trig", "eval", "manifest.jsonld"],
        ["turtle", "syntax", "manifest.jsonld"],
        ["turtle", "eval", "manifest.jsonld"],
        ["nt", "syntax", "manifest.jsonld"],
        ["sparql", "syntax", "manifest.jsonld"],
        ["sparql", "eval", "manifest.jsonld"]
    ]:
        make_html(DIR.joinpath(*i))

def make_html(path: Path):
    dir = path.parent
    html = dir.joinpath(path.stem + '.html')
    manifest = open_manifest(path)
    if manifest is None:
        eprint(f"Don't know how to parse {path}")
        return

    print(html)
    with html.open('w') as out:
        label = manifest.get('label').get('en')
        creator = manifest.get('creator')[0].get('foaf:name')
        issued = manifest.get('issued')
        modified = manifest.get('modified')

        out.write("<!DOCTYPE html>\n")
        out.write('<html lang="en">')
        out.write('<meta charset="UTF-8">')
        out.write(STYLE)
        out.write(f"<title>{label}</title>\n")
        out.write(f"<h1>{label}</h1>\n")
        if 'comment' in manifest:
            out.write(f"<p>{manifest['comment']}</p>\n")
        out.write('<table class="properties">\n')
        out.write(f'<tr class="generated"><th>Generated from:</th><td><a href="{path.name}">{path.name}</a></td>\n')
        out.write(f'<tr class="creator"><th>creator:</th><td>{creator}</td>\n')
        out.write(f'<tr class="issued"><th>issued:</th><td>{issued}</td>\n')
        out.write(f'<tr class="modified"><th>modified:</th><td>{modified}</td>\n')
        if 'seeAlso' in manifest:
            out.write(f'<tr class="seeAlso"><th>see also:</th><td><a href="{manifest["seeAlso"]}">{manifest["seeAlso"]}</a></td></tr>\n')
        out.write('</table>\n')

        include = manifest.get('include')
        if include:
            out.write('<p><strong>Includes:</strong></p>\n<ul class="inclueded">\n')
            for url in include:
                make_html(dir.joinpath(*url.split('/')))
                name = url.replace('.jsonld', '')
                url = url.replace('.jsonld', '.html')
                out.write(f'<li><a href="{url}">{name}</a>\n')
            out.write("</ul>\n")

        entries = manifest.get('entries') or []
        if entries:
            out.write('<p><strong>Entries:</strong></p>\n<ul class="entries">\n')
            for (i, entry) in enumerate(entries):
                eid = get_entry_id(entry, f"#{path.name}_entry{i}")
                name = entry.get('name', eid[1:])
                approval = entry.get('approval', 'proposed').lower()
                if approval == "notclassified":
                    approval = "proposed"
                out.write(f'<li class="{approval}"><a href="{eid}">{name}</a>\n')
                # store computed values for the next loop
                entry['@id'] = eid
                entry['name'] = name
                #
            out.write("</ul>\n")

        see_also = manifest.get('seeAlso')
        if see_also:
            out.write('<h2>About this test suite</h2>\n')
            if see_also.startswith('http://') or see_also.startswith('https://'):
                out.write(f'<a href="{see_also}">{see_also}</a>\n')
            else:
                see_also = dir.joinpath(*see_also.split('/'))
                out.write('<pre>\n')
                out.write(see_also.read_text())
                out.write('</pre>\n')

        for entry in entries:
            eid = entry['@id']
            typ = entry['@type']
            name = entry['name']
            approval = entry.get('approval', 'proposed').lower()
            if approval == "notclassified":
                approval = "proposed"
            out.write(f'<section id="{eid[1:]}" class="entry {approval} {typ}">\n')
            out.write(f'<h2>{name} <a href="{eid}">🔗</a></h2>\n')
            if 'comment' in entry:
                out.write(f"<p>{entry['comment']}</p>\n")
            out.write('<table class="properties">\n')
            out.write(f'<tr class="status"><th>status:</th><td>{approval}</td>\n')
            out.write(f'<tr class="type"><th>type:</th><td>{readable_type(entry)}</td>\n')
            if 'entailmentRegime' in entry:
                out.write(f'<tr class="regime"><th>regime:</th><td>{entry["entailmentRegime"]}</td>\n')
            recognized = entry.get('recognizedDatatypes')
            if recognized:
                out.write(f'<tr class="recognized"><th>recognizing:</th><td>{" ".join(recognized)}</td>\n')
            unrecognized = entry.get('unrecognizedDatatypes')
            if unrecognized:
                out.write(f'<tr class="unrecognized"><th>ignoring:</th><td>{" ".join(unrecognized)}</td>\n')
            out.write("</table>\n")

            write_action(out, entry['action'], dir)
            out.write(f"<div>{result_message(entry)}</div>\n")
            result = entry.get('result') or entry.get('mf:result')
            if isinstance(result, dict):
                result = next(iter(result.values()))
            if result is False:
                out.write("<div>a contradiction</div>")
            elif result:
                write_file(out, result, dir, cls="result")
            out.write("</section>")
        out.write('</html>')

def get_entry_id(entry: dict, default: str) -> str:
    eid = entry.get('@id', '')
    if not eid.startswith('#'):
        if eid.startswith('trs:'):
            eid = f"#{eid[4:]}"
        else:
            eid = ''
    return eid or default

def readable_type(entry: dict) -> str:
    typ = entry['@type']
    return {
        'PositiveEntailmentTest': 'positive entailment test',
        'NegativeEntailmentTest': 'negative entailment test',
    }.get(typ, typ)

def write_file(out, relative_url, current_dir, *, title=None, cls=None):
    if title is None:
        title = ""
    else:
        title  = f"{title}: "
    out.write(f'<div>{title}<code><a href="{relative_url}">{relative_url}</a></code></div>\n')
    try:
        with current_dir.joinpath(*relative_url.split('/')).open() as f:
            out.write("<pre")
            if cls is not None:
                out.write(f" class=\"{cls}\"")
            out.write(">\n")
            quoted = f.read().replace('&', '&amp;').replace('<', '&lt;').replace('>', '&gt;')
            out.write(quoted)
    except Exception as e:
        msg = "(problem rendering file) %s\n" % e
        out.write(msg)
        print(f"{relative_url}:", msg)
    finally:
        out.write("</pre>\n")

def write_action(out, action, current_dir, *, cls=None):
    if isinstance(action, str):
        write_file(out, action, current_dir, cls=cls)
        return
    assert isinstance(action, dict)
    assert 'qt:query' in action and 'qt:data' in action \
        or 'ut:request' in action and 'ut:data' in action, action
    if "qt:query" in action:
        write_file(out, action['qt:query'], current_dir, title="Query", cls=cls)
    else:
        write_file(out, action['ut:request'], current_dir, title="Request", cls=cls)
    data = action.get("qt:data") or action.get("ut:data")
    write_file(out, data, current_dir, title="Data", cls=cls)


def result_message(entry: dict) -> str:
    typ = entry['@type']

    if "PositiveEntailment" in typ:
        return "MUST entail"
    elif "NegativeEntailment" in typ:
        return "MUST NOT entail"
    elif "PositiveSyntax" in typ:
        return "MUST be accepted"
    elif "NegativeSyntax" in typ:
        return "MUST be rejected"
    elif "Negative" in typ:
        return "MUST NOT result into"
    return "MUST result into"

def open_manifest(path: Path) -> dict:
    with path.open() as f:
        return json.load(f)

def eprint(*args, **kw):
    kw.setdefault('file', sys.stderr)
    print(*args, **kw)

STYLE = '''
<style>
    .included a, .entries a {
        text-decoration: none;
    }

    .included a:hover, .entries a:hover {
        text-decoration: underline;
    }

    .entries .rejected {
        text-decoration: line-through red;
    }

    .entries .approved::after {
        content: "✓";
    }

    .entry h2 a {
        text-decoration: none;
    }

    .approved tr.status td {
        color: darkGreen;
    }

    .proposed tr.status td {
        color: orange;
    }

    .rejected tr.status td {
        color: red;
    }

    tr.recognized td, tr.unrecognized td {
        font-family: monospace;
    }

    pre {
        border: thin solid black;
        background-color: lightYellow;
        padding: .6em 1em;
        max-height: 25em;
        overflow-y: scroll;
    }

    .TestTurtleNegativeSyntax pre,
    .NegativeSyntaxTest11 pre,
    .NegativeUpdateSyntaxTest11 pre,
    .NegativeEntailmentTest pre.result {
        background-color: lightPink;
    }
</style>
''' 

main()
