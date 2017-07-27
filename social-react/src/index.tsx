import * as React from 'react';
import * as ReactDOM from 'react-dom';
import registerServiceWorker from './registerServiceWorker';
import './index.css';
import { Provider } from 'react-redux';
import { createStore } from 'redux';
import { reducers } from './store';
import FilterList from './components/FilterList/FilterList'

// function buildRootReducer(allReducers: any) {
//   return combineReducers<ApplicationState>(Object.assign({}, allReducers, {}));
// }
// const allReducers = buildRootReducer(reducers);
const store = createStore(reducers.filterList);

ReactDOM.render(
  <Provider store={store}>
    <FilterList />
  </Provider>,
  document.getElementById('root') as HTMLElement
);
registerServiceWorker();
