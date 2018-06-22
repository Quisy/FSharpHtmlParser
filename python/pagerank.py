from urllib.request import Request, urlopen
from urllib.error import HTTPError, URLError
from bs4 import BeautifulSoup
from collections import Counter
import validators
import ssl
import re
import pandas as pd
import networkx as nx
import matplotlib.pyplot as plt
import json
import requests

urls = {'http://pduch.kis.p.lodz.pl/'}
words_counters = []
words = Counter()
links = set()
depth = 2
source_nodes = []
target_nodes = []
page_ranks = dict()


def __walk_page(current_url, current_depth):
    print('CURRENT URL:', current_url)
    print('CURRENT DEPTH:', current_depth)
    __get_words(current_url)
    __get_links(current_url)

    if current_depth > 1:
        for url in __get_link_generator(current_url):
            if '.exe' not in url:
                if __make_raw_url(url) not in urls:
                    urls.add(__make_raw_url(url))
                    __walk_page(url, current_depth - 1)
                source_nodes.append(__make_raw_url(current_url))
                target_nodes.append(__make_raw_url(url))
    elif current_depth is 1:
        for url in __get_link_generator(current_url):
            if '.exe' not in url:
                if __make_raw_url(url) not in urls:
                    source_nodes.append(__make_raw_url(current_url))
                    target_nodes.append(__make_raw_url(url))


def __load_page(url):
    req = Request(url, headers={'User-Agent': 'Mozilla/5.0'})
    with urlopen(req, context=ssl._create_unverified_context()) as context:
        soup = BeautifulSoup(context, 'html.parser')
    return soup


def __get_words(url):
    try:
        soup = __load_page(url)
        for script in soup(['script', 'style']):
            script.extract()
        text = list(filter(None, re.sub('[\s\-–=&:,!|„”"\'`@\?;/\$%\\\(\)\{\}\[\]\.\+\*0-9]+', ' ',
                                        soup.get_text()).lower().split(' ')))
        words.update(Counter(text))
    except (HTTPError, URLError, UnicodeEncodeError, TimeoutError):
        pass


def __get_links(url):
    try:
        for tag in __load_page(url).findAll('a'):
            try:
                link = tag['href']
                if validators.url(link):
                    links.add(link)
            except KeyError:
                pass
    except (HTTPError, URLError, UnicodeEncodeError, TimeoutError):
        pass


def __get_link_generator(url):
    try:
        for tag in __load_page(url).findAll('a'):
            try:
                if validators.url(tag['href']):
                    yield tag['href']
            except KeyError:
                pass
    except (HTTPError, URLError, UnicodeEncodeError, TimeoutError):
        pass


def __make_raw_url(url):
    return re.sub('www.', '', str(url).split('//')[1].split('/')[0].split('?')[0])


def __print_graph():
    print(source_nodes)
    print(target_nodes)
    df = pd.DataFrame({'from': source_nodes, 'to': target_nodes})
    graph = nx.from_pandas_edgelist(df, 'from', 'to', create_using=nx.DiGraph())
    pr = nx.pagerank(graph, alpha=0.85)
    pages = list(pr.keys())
    ranks = list(pr.values())
    carac = pd.DataFrame({'page': pages, 'rank': ranks})
    graph.nodes()
    carac = carac.set_index('page')
    carac = carac.reindex(graph.nodes())
    nx.draw(graph, with_labels=True, arrows=True, alpha=0.85, node_color=carac['rank'],
            cmap=plt.cm.Blues, node_size=750, linewidths=5)
    fig, ax = plt.subplots()
    fig.set_tight_layout(False)
    plt.show()
    print('PAGE RANK:', pr)
    global page_ranks
    page_ranks = pr


class Site:
    def __init__(self, url, pageRank, words):
        self.url = url
        self.pagerank = pageRank
        self.words = words


# plt.set_tight_layout(False)

for url in urls:
    words_counters = []
    words = Counter()
    links = set()
    source_nodes = []
    target_nodes = []
    page_ranks = dict()

    __walk_page(current_url=url, current_depth=depth)
    __print_graph()

    print('PAGE: ' + url)
    print('WORDS:')
    # print(words)
    words_list = []
    words_counters.append(dict(words.most_common()))
    for entry in words.most_common():
        words_list.append(entry[0])
        # print(entry[0] + ': ' + str(entry[1]))

    # for link in links:
    #     print(link)

    words_json = json.dumps(words_list)
    x = Site(url, page_ranks[__make_raw_url(url)], words_list)
    json_data = json.dumps(x.__dict__)
    print(json_data)
    headers = {'Content-type': 'application/json', 'Accept': 'text/plain'}
    r = requests.post("http://searcher.quisy.hostingasp.pl/api/sites", data=json_data, headers=headers)
    print(r.status_code, r.reason)
