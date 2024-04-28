
// adding missing tables
currentCollections = db.getCollectionNames();

const _id = {
    bsonType: 'objectId'
};

const stringSchema = {
    bsonType: 'string',
    minLength: 1
};

const intSchema = {
    bsonType: 'int'
};

const boolSchema = {
    bsonType: 'bool'
}

const cardSchema = {
    bsonType: 'object',
    properties: {
        ImageId: intSchema,
        IsSuggested: boolSchema,
        CardType: {
            bsonType: 'int',
            minimum: 0,
            maximum: 2
        }
    },
    required: [
        'Text',
        'ImageId',
        'IsSuggested',
        'CardType'
    ],
    additionalProperties: false
};

const cardCollectionSchema = {
    bsonType: 'array',
    items: cardSchema
};

const deckSchema = {
    bsonType: 'array',
    items: intSchema
};

const updatedValidators = {
    Games: {
        $jsonSchema: {
            properties: {
                _id,
                GameId: {
                    bsonType: 'binData'
                },
                HostId: {
                    bsonType: 'binData'
                },
                Initialization: {
                    bsonType: 'date'
                },
                PlayerIds: {
                    bsonType: 'array',
                    items: {
                        bsonType: 'binData'
                    }
                },
                InnocentDeck: deckSchema,
                GuiltyDeck: deckSchema,
                ModifierDeck: deckSchema,
                DiscardedCards: cardCollectionSchema,
                RoundNumber: intSchema,
                Discussion: {
                    bsonType: 'array',
                    items: {
                        bsonType: 'object',
                        properties: {
                            Name: stringSchema,
                            Timestamp: {
                                bsonType: 'date'
                            },
                            Message: stringSchema
                        },
                        additionalProperties: false
                    }
                },
                Track: {
                    bsonType: 'object',
                    properties: {
                        LeftTrack: deckSchema,
                        RightTrack: deckSchema,
                    },
                    additionalProperties: false
                },
                LastAction: {
                    bsonType: 'date'
                },
            },
            required: [
                'GameId',
                'HostId',
                'Initialization',
                'PlayerIds',
                'InnocentDeck',
                'GuiltyDeck',
                'ModifierDeck',
                'DiscardedCards',
                'RoundNumber',
                'Discussion',
                'Track',
                'LastAction'
            ],
            additionalProperties: false
        }
    },
    Players: {
        $jsonSchema: {
            properties: {
                _id,
                PlayerId: {
                    bsonType: 'binData'
                },
                GameId: {
                    bsonType: 'binData'
                },
                Name: stringSchema,
                IsHost: boolSchema,
                IsConductor: boolSchema,
                RoundsWon: intSchema,
                Hand: {
                    bsonType: 'object',
                    properties: {
                        InnocentCards: cardCollectionSchema,
                        GuiltyCards: cardCollectionSchema,
                        ModifierCards: cardCollectionSchema
                    },
                    required: [
                        'InnocentCards',
                        'GuiltyCards',
                        'ModifierCards'
                    ],
                    additionalProperties: false
                }
            },
            required: [
                'PlayerId',
                'GameId',
                'Name',
                'IsHost',
                'IsConductor',
                'RoundsWon',
                'Hand'
            ],
            additionalProperties: false
        }
    },
    Cards:{
        $jsonSchema: {
            properties: {
                _id: {
                    bsonType: 'string'
                },
                InnocentDeck: deckSchema,
                GuiltyDeck: deckSchema,
                ModifierDeck: deckSchema
            },
            required: [
                'InnocentDeck',
                'GuiltyDeck',
                'ModifierDeck'
            ],
            additionalProperties: false
        }
    }
};

// update schema validation
for (const collection in updatedValidators) {
  if (!currentCollections.includes(collection)){
    console.log(`Adding new collection ${collection}`);
    db.createCollection(collection);
  }

  console.log(`Updating schema for ${collection} colllection`);
  db.runCommand({
      collMod: collection,
      validator: updatedValidators[collection]
  });
}

console.log('Updating the Cards collection');

const innocentDeck = [6,60,61,62,63,64,65,66,67,68,69,7,70,71,72,73,74,75,76,77,78,79,8,80,81,82,83,84,85,86,87,88,89,9,92,93,94,95,96,98];
const guiltyDeck = [1,10,100,101,102,103,104,105,106,107,108,109,11,110,111,112,113,114,115,116,117,118,119,12,120,121,122,123,124,125,13,14,15,16,17,18,19,2,20,21];
const modifierDeck = [0,10,100,101,102,103,104,105,106,107,108,109,11,110,111,112,113,114,115,116,117,118,119,12,120,121,122,123,124,125,126,127,128,129,13,130,131,132,133,134];

const cardCollectionIndex = db.Cards.countDocuments();
if (cardCollectionIndex < 1){
    db.Cards.insertOne({
        _id: 'cards',
        'InnocentDeck': [],
        'GuiltyDeck': [],
        'ModifierDeck': []
    });
}

const currentCardCollection = db.Cards.findOne({_id: {$eq: 'cards'}});
currentCardCollection.InnocentDeck = innocentDeck;
currentCardCollection.GuiltyDeck = guiltyDeck;
currentCardCollection.ModifierDeck = modifierDeck;

const query = {_id: 'cards'};
const update = {$set: currentCardCollection}
const options = { upsert: true };
db.Cards.updateOne(query, update, options);