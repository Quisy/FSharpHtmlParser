import nltk
from nltk.tokenize import word_tokenize
import xml.etree.ElementTree

import re


def cleanhtml(raw_html):
    cleanr = re.compile('<.*?>')
    cleantext = re.sub(cleanr, '', raw_html)
    return cleantext

# nltk.download('punkt')


e = xml.etree.ElementTree.parse('Posts_literature.xml').getroot()

testList = [cleanhtml(x.get('Body')) for x in e.findall('row')]
testList = testList[:20]


train_data_file = open('train_data.txt', 'r')
train_data = train_data_file.readlines()

train_data_pos = [x for x in train_data if x.split('\t')[0] == '1'][:100]
train_data_neg = [x for x in train_data if x.split('\t')[0] == '0'][:100]

train_data = train_data_pos + train_data_neg

train_set_data = {(x.split('\t')[1].replace('\n', ''), x.split('\t')[0]) for x in train_data}

train = train_set_data
all_train_words = set(word.lower() for passage in train for word in word_tokenize(passage[0]))
train_set = [({word: (word in word_tokenize(x[0])) for word in all_train_words}, x[1]) for x in train]

# test_sentence = "This is the best band I've ever heard!"
# test_sent_features = {word.lower(): (word in word_tokenize(test_sentence.lower())) for word in all_train_words}

classifier = nltk.NaiveBayesClassifier.train(train_set)

for test_data in testList:
    print(test_data)
    test_sent_features = {word.lower(): (word in word_tokenize(test_data.lower())) for word in all_train_words}
    print(classifier.classify(test_sent_features))

