import { combineReducers } from 'redux';
import filterList from './filterList';
import conversationList from './conversationList';

const reducers = combineReducers({ filterList, conversationList })
export default reducers;