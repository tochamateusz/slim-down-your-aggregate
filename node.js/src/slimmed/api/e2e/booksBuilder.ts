import request from 'supertest';
import { faker } from '@faker-js/faker';
import { Application } from 'express';
import { TestResponse } from '#testing/api/testResponse';
import { config } from '#config';

export const createDraft = async (app: Application) => {
  const body = {
    title: faker.string.sample(),
    author: {
      firstName: faker.person.firstName(),
      lastName: faker.person.lastName(),
    },
    publisherId: config.application.existingPublisherId,
    edition: faker.number.int({ min: 0 }),
    genre: faker.string.sample(),
  };

  const response = (await request(app)
    .post('/api/books/')
    .send(body)
    .expect(201)) as TestResponse<{ id: string }>;

  const current = response.body;

  if (!current.id) {
    expect(false).toBeTruthy();
    throw 'not gonna happen!';
  }
  expect(current.id).toBeDefined();

  return { id: current.id, ...body };
};

export const addChapter = async (bookId: string, app: Application) => {
  const body = {
    title: 'Chapter 1 - ' + faker.string.sample(),
    content: faker.string.sample(),
  };

  await request(app)
    .post(`/api/books/${bookId}/chapters`)
    .send(body)
    .expect(204);

  return body;
};
