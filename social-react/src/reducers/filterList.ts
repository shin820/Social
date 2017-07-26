import { FilterAction } from '../actions';
import { RELOAD_FILTER_LIST } from '../actions/index';
import { FilterListItem } from '../types/index';

export default function filterList(state: FilterListItem[], action: FilterAction): FilterListItem[] {
    switch (action.type) {
        case RELOAD_FILTER_LIST:
            return [new FilterListItem(1, "test", 10), new FilterListItem(2, "test2", 20)];
    }
    return state;
}