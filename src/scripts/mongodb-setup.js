
// adding missing tables
currentCollections = db.getCollectionNames();

const _id = {
    bsonType: 'objectId'
};

const cardSchema = {
    bsonType: 'object',
    properties: {
        Text: {
            bsonType: 'string',
            minLength: 1
        },
        ImageId: {
            bsonType: 'string'
        },
        CardType: {
            bsonType: 'int',
            minimum: 0,
            maximum: 2
        }
    },
    required: [
        'Text',
        'ImageId',
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
                Name: {
                    bsonType: 'string',
                    minLength: 1
                },
                IsHost: {
                    bsonType: 'bool'
                },
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