
// adding missing tables
currentCollections = db.getCollectionNames();

const _id = {
    bsonType: 'objectId'
};

const stringSchema = {
    bsonType: 'string',
    minLength: 1
};

const boolSchema = {
    bsonType: 'bool'
}

const cardSchema = {
    bsonType: 'object',
    properties: {
        Text: stringSchema,
        ImageId: stringSchema,
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

const deckSchema = {
    bsonType: 'array',
    items: cardSchema
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
                DiscardedCards: deckSchema,
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
                Hand: {
                    bsonType: 'object',
                    properties: {
                        InnocentCards: deckSchema,
                        GuiltyCards: deckSchema,
                        ModifierCards: deckSchema
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
                'Hand'
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
  })
}