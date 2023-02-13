import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { Todo } from '../shared/todo.model';
import {DataService} from "../shared/data.service";

@Component({
  selector: 'app-todo-item',
  templateUrl: './todo-item.component.html',
  styleUrls: ['./todo-item.component.scss']
})
export class TodoItemComponent implements OnInit {
  @Input() todo: Todo | undefined
  @Output() refresh: EventEmitter<void> = new EventEmitter<void>()

  constructor(private dataService: DataService) { }

  ngOnInit(): void {
  }

  onTodoClicked() {
    if(this.todo){
      this.dataService.updateTodo(this.todo.id)
        .subscribe();
      this.todo.done = true;
    }
  }

  onEditClicked() {
  }

  onDeleteClicked() {
    if(this.todo) {
      this.dataService.deleteTodo(this.todo.id)
        .subscribe(() => this.refresh.emit());
    }
  }

}
