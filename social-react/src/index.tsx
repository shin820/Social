import * as React from 'react';
import * as ReactDOM from 'react-dom';
import registerServiceWorker from './registerServiceWorker';
import './index.css';
import { Provider } from 'react-redux';
import { createStore } from 'redux';
import { reducers } from './store';
import FilterList from './components/FilterList/FilterList'

const store = createStore(reducers);

ReactDOM.render(
  <Provider store={store}>
    <FilterList />
  </Provider>,
  document.getElementById('root') as HTMLElement
);
registerServiceWorker();
