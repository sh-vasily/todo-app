import { Component, OnInit } from '@angular/core';
import { Todo } from '../shared/todo.model';
import { DataService } from '../shared/data.service';
import { NgForm } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { Observable } from 'rxjs';
import  { switchMap  } from 'rxjs/operators';
import {EditTodoDialogComponent} from "../edit-todo-dialog/edit-todo-dialog.component";
@Component({
  selector: 'app-todos',
  templateUrl: './todos.component.html',
  styleUrls: ['./todos.component.scss']
})
export class TodosComponent implements OnInit {

  todos:  Observable<Todo[]> = new Observable<Todo[]>();
  showValidationErrors: boolean | undefined
  showDone: boolean = false;

  constructor(private dataService: DataService, private dialog: MatDialog) {
    this.getAllTodos();

  }

  ngOnInit(): void {
    this.getAllTodos();
    }


  onFormSubmit(form: NgForm) {
    this.todos = this.dataService.addTodo(form.value.text)
      .pipe(
        switchMap(() => this.dataService.getAllTodos())
      );
    if (form.invalid)
      return this.showValidationErrors = true

    this.dataService.addTodo(form.value.text);

    this.showValidationErrors = false
    form.reset()
    return false;
  }

  toggleCompleted(todo: Todo) {
    this.dataService.updateTodo(todo.id)
      .subscribe(x => this.getAllTodos());
  }

  editTodo(todo: Todo) {
    let dialogRef = this.dialog.open(EditTodoDialogComponent, {
      width: '700px',
      data: todo
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.dataService.updateTodo(todo.id)
      }
    })
  }

  getAllTodos() {
    this.todos = this.showDone
      ? this.dataService.getDoneTodos()
      : this.dataService.getAllTodos();
  }
}
