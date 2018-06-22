from functools import reduce

import nltk
from nltk.tokenize import word_tokenize




# single_train_data = [('sql', ['sql', 'database'])
#                      ]

multiple_train_data = [('.net c# wpf uwp asp.net xamarin', ['.net', 'c#']),
                       ('java spring hibernate', ['java']),
                       ('autofac windsor simpleinjector', ['ioc', 'container']),
                       ('postgre sqlserver mongo cassandra oracle mysql sql', ['database']),
                       ('aws azure', ['cloud']),
                       ]

train_data_list = []
# for x in single_train_data:
#     for tag in x[1]:
#         train_data_list.append((x[0], tag))

for x in multiple_train_data:
    for tag in x[1]:
        train_data_list.append((x[0], tag))
    for tag in x[0].split(' '):
        train_data_list.append((tag, tag))

train = train_data_list
all_train_words = set(word.lower() for passage in train for word in word_tokenize(passage[0]))
train_set = [({word: (word in word_tokenize(x[0])) for word in all_train_words}, x[1]) for x in train]

test_sentence = """I am trying to implement Dependency Injection with Autofac  in an ASP.NET MVC5 Project. But I am getting the following error every time:. Is this sql problem?"""
test_sent_features = {word.lower(): (word in word_tokenize(test_sentence.lower())) for word in all_train_words}

classifier = nltk.NaiveBayesClassifier.train(train_set)

result = classifier.prob_classify(test_sent_features)
probResults = []

for tag in result.samples():
    probResults.append((tag, result.prob(tag)))
    # print(tag + ': ' + str(result.prob(tag)))

values = [x[1] for x in probResults]

avg = reduce(lambda x, y: x + y, values) / len(values)

for x in probResults:
    if float(x[1]) > avg * 2:
        print(x[0])
