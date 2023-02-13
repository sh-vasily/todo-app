import { Injectable } from '@angular/core';
import { Todo } from './todo.model';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class DataService {
  constructor(private httpClient: HttpClient) { }

  getAllTodos() {
    return this.httpClient.get<Todo[]>("/api/todo");
  }

  getDoneTodos() {
    return this.httpClient.get<Todo[]>("/api/todo?tags=done");
  }

  addTodo(value: string) {
    let [description, comment] = value.split(';');

    return this.httpClient
      .post("/api/todo",{
        description: description,
        comment: comment
      })
  }

  updateTodo(id: string) {
    return this.httpClient
      .put(`/api/todo/${id}`, null)
  }

  deleteTodo(id: string) {
    return this.httpClient
      .delete(`/api/todo/${id}`);
  }

}
